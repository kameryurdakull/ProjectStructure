using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using EventSystem;
using Game.Core.PostProcessing;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VContainer;
using Object = UnityEngine.Object;

namespace PackageStructure.Validation
{
    public sealed class ToolkitCompatibilityTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private const float Baseline = 0.5f;
        private GameObject _root;
        private VolumeProfile _sourceProfile;
        private VolumeProfile _runtimeProfile;
        private Volume _volume;
        private Bloom _bloom;
        private PostProcessVolumeExtension _service;
        private EventBus _hostBus;
        private HostBusAdapter _adapter;
        private readonly List<Object> _temporaryAssets = new();

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject(nameof(ToolkitCompatibilityTests));
            _root.SetActive(false);
            _sourceProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            _volume = _root.AddComponent<Volume>();
            _volume.sharedProfile = _sourceProfile;
            _volume.weight = Baseline;
            _runtimeProfile = _volume.profile;
            _bloom = _runtimeProfile.Add<Bloom>(true);
            _bloom.intensity.value = Baseline;
            _bloom.intensity.overrideState = false;
            _service = _root.AddComponent<PostProcessVolumeExtension>();
            _hostBus = new EventBus();
            _adapter = new HostBusAdapter(_hostBus);
            _service.Construct(_adapter);
            _root.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_runtimeProfile);
            Object.DestroyImmediate(_sourceProfile);
            foreach (var asset in _temporaryAssets) Object.DestroyImmediate(asset);
            _temporaryAssets.Clear();
        }

        [Test]
        public void SharedEventBusTypesHaveExactlyOneProvider()
        {
            foreach (var name in new[] { typeof(IEventBus).FullName, typeof(EventBus).FullName })
            {
                var providers = AppDomain.CurrentDomain.GetAssemblies().Where(assembly => assembly.GetType(name) != null).ToArray();
                Assert.That(providers, Has.Length.EqualTo(1));
                Assert.That(providers[0].GetName().Name, Is.EqualTo(typeof(EventBus).Assembly.GetName().Name));
            }
        }

        [Test]
        public void DirectSetAndResetRestoreValueAndOverrideState()
        {
            _service.Set(PostProcessEffect.BloomIntensity, 2f, 0f);
            Assert.That(_bloom.intensity.value, Is.EqualTo(2f));
            Assert.That(_bloom.intensity.overrideState, Is.True);
            _service.Reset(PostProcessEffect.BloomIntensity, 0f);
            Assert.That(_bloom.intensity.value, Is.EqualTo(Baseline));
            Assert.That(_bloom.intensity.overrideState, Is.False);
        }

        [Test]
        public void SharedHostBusRoutesRequestsThroughAdapter()
        {
            _hostBus.Publish(new PostProcessRequestedEvent(PostProcessRequest.Set(PostProcessEffect.BloomIntensity, 3f, 0f)));
            Assert.That(_bloom.intensity.value, Is.EqualTo(3f));
        }

        [Test]
        public void VContainerResolvesAndInjectsHostAdapter()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance<IPostProcessEventBus>(_adapter);
            using (var resolver = builder.Build()) resolver.Inject(_service);
            _hostBus.Publish(new PostProcessRequestedEvent(PostProcessRequest.Set(PostProcessEffect.BloomIntensity, 4f, 0f)));
            Assert.That(_bloom.intensity.value, Is.EqualTo(4f));
            Assert.That(_adapter.SubscriptionCount, Is.EqualTo(2));
        }

        [Test]
        public void DisableUnsubscribesAndReenableResubscribesOnce()
        {
            _service.enabled = false;
            Assert.That(_adapter.SubscriptionCount, Is.Zero);
            _hostBus.Publish(new PostProcessRequestedEvent(PostProcessRequest.Set(PostProcessEffect.BloomIntensity, 4f, 0f)));
            Assert.That(_bloom.intensity.value, Is.EqualTo(Baseline));
            _service.enabled = true;
            _service.Construct(_adapter);
            Assert.That(_adapter.SubscriptionCount, Is.EqualTo(2));
            _hostBus.Publish(new PostProcessRequestedEvent(PostProcessRequest.Set(PostProcessEffect.BloomIntensity, 4f, 0f)));
            Assert.That(_bloom.intensity.value, Is.EqualTo(4f));
        }

        [Test]
        public void PulseTweenReachesPeakAndRestoresBaseline()
        {
            _service.Pulse(PostProcessEffect.BloomIntensity, 2f, 0.1f, 0.2f);
            var tweens = (IDictionary)typeof(PostProcessVolumeExtension).GetField("_activeTweens", PrivateInstance).GetValue(_service);
            var tween = (Tween)tweens[PostProcessEffect.BloomIntensity];
            tween.Goto(0.1f);
            Assert.That(_bloom.intensity.value, Is.EqualTo(2f).Within(0.001f));
            tween.Goto(0.31f);
            Assert.That(_bloom.intensity.value, Is.EqualTo(Baseline).Within(0.001f));
            Assert.That(_bloom.intensity.overrideState, Is.False);
        }

        [Test]
        public void BlendWeightClampsAndResetAllRestoresState()
        {
            _service.BlendWeight(5f, 0f);
            Assert.That(_volume.weight, Is.EqualTo(1f));
            _service.BlendWeight(-5f, 0f);
            Assert.That(_volume.weight, Is.Zero);
            _service.Set(PostProcessEffect.BloomIntensity, 3f, 0f);
            _service.ResetAll(0f);
            Assert.That(_volume.weight, Is.EqualTo(Baseline));
            Assert.That(_bloom.intensity.value, Is.EqualTo(Baseline));
        }

        [Test]
        public void PresetEventsCreateAndRemovePresetVolumes()
        {
            var preset = CreatePreset(false);
            _hostBus.Publish(new PostProcessPresetRequestedEvent(PostProcessPresetOperation.Play, preset, preset.Channel, 0f));
            Assert.That(ActivePresets.Count, Is.EqualTo(1));
            var volumes = _root.GetComponentsInChildren<Volume>();
            Assert.That(volumes.Any(volume => volume.sharedProfile == preset.Profile && volume.weight == 1f), Is.True);
            _hostBus.Publish(new PostProcessPresetRequestedEvent(PostProcessPresetOperation.StopPreset, preset, preset.Channel, 0f));
            Assert.That(ActivePresets.Count, Is.Zero);
        }

        [Test]
        public void StackablePresetsCoexistAndResetClearsThem()
        {
            _service.PlayPreset(CreatePreset(true), 0f);
            _service.PlayPreset(CreatePreset(true), 0f);
            Assert.That(ActivePresets.Count, Is.EqualTo(2));
            _service.ResetPresets(0f);
            Assert.That(ActivePresets.Count, Is.Zero);
        }

        private IDictionary ActivePresets => (IDictionary)typeof(PostProcessVolumeExtension)
            .GetField("_activePresets", PrivateInstance).GetValue(_service);

        private PostProcessPreset CreatePreset(bool stackable)
        {
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.Add<Bloom>(true);
            var preset = ScriptableObject.CreateInstance<PostProcessPreset>();
            typeof(PostProcessPreset).GetField("_profile", PrivateInstance).SetValue(preset, profile);
            typeof(PostProcessPreset).GetField("_stackable", PrivateInstance).SetValue(preset, stackable);
            _temporaryAssets.Add(preset);
            _temporaryAssets.Add(profile);
            return preset;
        }

        private sealed class HostBusAdapter : IPostProcessEventBus
        {
            private readonly IEventBus _host;
            internal int SubscriptionCount { get; private set; }
            internal HostBusAdapter(IEventBus host) => _host = host;
            public void Subscribe<T>(Action<T> callback) { _host.Subscribe(callback); SubscriptionCount++; }
            public void Unsubscribe<T>(Action<T> callback) { _host.Unsubscribe(callback); SubscriptionCount--; }
            public void Publish<T>(T eventMessage) => _host.Publish(eventMessage);
        }
    }
}

﻿using System.Collections;
using RuntimeUnitTestToolkit;
using UnityEngine;
using UniRx;

namespace Hado.Utils.ObjectPool.UnitTests
{
    public class ObjectPoolEventFunctionsTest : ObjectPoolTestBase
    {
        IObjectPool<EventFunctionsReceiver> CreatePool(bool prefabEnable, int numberOfInstances, bool needPreactivation)
        {
            var prefab = Resources.Load("EventFunctionsReceiver") as GameObject;
            prefab.gameObject.SetActive(prefabEnable);
            var component = prefab.GetComponent<EventFunctionsReceiver>();
            var config = new ObjectPoolConfig(
                numberOfInstances: numberOfInstances,
                createCountPerFrame: numberOfInstances,
                needPreactivation: needPreactivation
            );
            return base.CreatePool(component, config);
        }

        public IEnumerator EnabledPrefabsStartEventIsNotCalledBeforeRent()
        {
            var pool = CreatePool(prefabEnable: true, numberOfInstances: 1, needPreactivation: false);
            yield return pool.PreloadAsync().ToYieldInstruction();
            yield return pool.PreactivateAsync().ToYieldInstruction();
            var receiver = pool.Rent();
            receiver.IsAwakeCalled.IsTrue();
            receiver.OnEnableCount.Is(1);
            receiver.IsStartCalled.IsFalse();
            receiver.OnDisableCount.Is(0);
            yield return null;
            receiver.IsStartCalled.IsTrue();
            pool.Return(receiver);
        }

        public IEnumerator DisabledPrefabsStartEventIsNotCalledBeforeRent()
        {
            var pool = CreatePool(prefabEnable: false, numberOfInstances: 1, needPreactivation: false);
            yield return pool.PreloadAsync().ToYieldInstruction();
            yield return pool.PreactivateAsync().ToYieldInstruction();
            var receiver = pool.Rent();
            receiver.IsAwakeCalled.IsTrue();
            receiver.OnEnableCount.Is(1);
            receiver.IsStartCalled.IsFalse();
            receiver.OnDisableCount.Is(0);
            yield return null;
            receiver.IsStartCalled.IsTrue();
            pool.Return(receiver);
            receiver.OnDisableCount.Is(1);
        }

        public IEnumerator PreactivationMakesCallStart()
        {
            var pool = CreatePool(prefabEnable: true, numberOfInstances: 1, needPreactivation: true);
            yield return pool.PreloadAsync().ToYieldInstruction();
            yield return pool.PreactivateAsync().ToYieldInstruction();
            var receiver = pool.Rent();
            receiver.IsAwakeCalled.IsTrue();
            receiver.OnEnableCount.Is(2);   // preactivate + rent
            receiver.IsStartCalled.IsTrue();
            receiver.OnDisableCount.Is(1);  // preactivate
            pool.Return(receiver);
        }

        public IEnumerator PreactivationMakesCallStartEvenIfPrefabDisable()
        {
            var pool = CreatePool(prefabEnable: false, numberOfInstances: 1, needPreactivation: true);
            yield return pool.PreloadAsync().ToYieldInstruction();
            yield return pool.PreactivateAsync().ToYieldInstruction();
            var receiver = pool.Rent();
            receiver.IsAwakeCalled.IsTrue();
            receiver.OnEnableCount.Is(2);   // preactivate + rent
            receiver.IsStartCalled.IsTrue();
            receiver.OnDisableCount.Is(1);  // preactivate
            pool.Return(receiver);
        }
    }
}
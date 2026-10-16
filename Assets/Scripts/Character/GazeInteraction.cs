using System;
using Architecture;
using Cysharp.Threading.Tasks;
using Event;
using QFramework;
using UnityEngine;

namespace XR
{
    public class GazeInteraction : MonoBehaviour, IController, ICanSendEvent
    {
        /// <summary>
        /// 在 Inspector 中选择你想检测的层
        /// </summary>
        [SerializeField] private LayerMask targetLayerMask;

        /// <summary>
        /// 用户视角位置，通常为主摄像机
        /// </summary>
        public Transform playerEyes;

        /// <summary>
        /// 射线长度
        /// </summary>
        public float rayLength = 1000f;

        /// <summary>
        /// 当前注视的对象
        /// </summary>
        private GameObject _currentLookAtObject;

        private async void Start()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            CheckTargetAsync().Forget();
        }

        private async UniTaskVoid CheckTargetAsync()
        {
            while (true)
            {
                var rayOrigin = playerEyes.position;
                var rayDirection = playerEyes.forward;

                if (Physics.Raycast(rayOrigin, rayDirection, out var hitInfo, rayLength, targetLayerMask))
                {
                    // 如果射线击中了一个新的物体
                    if (!_currentLookAtObject || hitInfo.collider.gameObject != _currentLookAtObject)
                    {
                        StartLookingAt(hitInfo.collider.gameObject);
                    }

                    _currentLookAtObject = hitInfo.collider.gameObject;
                }
                else
                {
                    // 如果没有击中任何物体，且之前有注视的对象，则表示视线离开了
                    if (_currentLookAtObject)
                    {
                        StopLookingAt();
                    }

                    _currentLookAtObject = null;
                }

                await UniTask.Yield();
            }
        }


        /// <summary>
        /// 在视线进入时执行的操作
        /// </summary>
        /// <param name="obj"></param>
        private void StartLookingAt(GameObject obj)
        {
            Debug.Log("开始看向: " + obj.name);
            this.SendEvent(new StartLookingAtEvent(obj));
        }

        /// <summary>
        /// 在视线离开时执行的操作
        /// </summary>
        private void StopLookingAt()
        {
            Debug.Log("视线离开: " + _currentLookAtObject.name);
            this.SendEvent(new StopLookingAtEvent(_currentLookAtObject));
            _currentLookAtObject = null;
        }

        /// <summary>
        /// 这个函数可以用来处理持续看向的逻辑，例如每秒调用一次
        /// </summary>
        private void FixedUpdate()
        {
            if (_currentLookAtObject != null)
            {
                ContinueLookingAt(_currentLookAtObject);
            }
        }

        private void ContinueLookingAt(GameObject obj)
        {
            // 处理持续看向的逻辑
            Debug.Log("正在持续看向: " + obj.name);
            // 可能包含一些持续的交互，如更新UI或触发某些效果
        }

        public IArchitecture GetArchitecture()
        {
            return PicoEyeArchitecture.Interface;
        }
    }
}
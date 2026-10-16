using System;
using Architecture;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Event;
using QFramework;
using UI;
using UnityEngine;

namespace Game
{
    public class ObservedObject : MonoBehaviour, IController
    {
        private Tween _tweener;

        private async void Start()
        {
            this.RegisterEvent<StartLookingAtEvent>(StartLookingAtEventCall);
            this.RegisterEvent<StopLookingAtEvent>(StopLookingAtEventCall);
        }

        private void OnDestroy()
        {
            this.UnRegisterEvent<StartLookingAtEvent>(StartLookingAtEventCall);
            this.UnRegisterEvent<StopLookingAtEvent>(StopLookingAtEventCall);
        }

        private void StartLookingAtEventCall(StartLookingAtEvent e)
        {
            if (e.Target != gameObject) return;
            e.Target.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
            IntroduceCanvas.Instance.SetToTargetPositionAndInfo(this);
            IntroduceCanvas.Instance.ShowAnim();
            HideAnim();
        }

        private void StopLookingAtEventCall(StopLookingAtEvent e)
        {
            if (e.Target != gameObject) return;
            e.Target.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
            IntroduceCanvas.Instance.HideAnim();
            ShowAnim();
        }


        private void ShowAnim()
        {
            _tweener?.Kill();
            _tweener = transform.GetChild(0).DOScale(1, 0.5f);
        }

        private void HideAnim()
        {
            _tweener?.Kill();
            _tweener = transform.GetChild(0).DOScale(0, 0.5f);
        }

        public IArchitecture GetArchitecture()
        {
            return PicoEyeArchitecture.Interface;
        }
    }
}
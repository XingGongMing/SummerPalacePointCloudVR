using System;
using System.Linq;
using DG.Tweening;
using Game;
using QFramework;
using ScriptObject;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Utils;
using Utils.Config;

namespace UI
{
    public class IntroduceCanvas : MonoSingleton<IntroduceCanvas>
    {
        /// <summary>
        /// 显示介绍图片
        /// </summary>
        [SerializeField] private RawImage showIntroduceImg;

        [SerializeField] private RawImage resourceIconType;
        [SerializeField] private Texture resourceVideoIcon;
        [SerializeField] private Texture resourceImgIcon;

        private Transform _playerTransform;

        private VideoPlayer _videoPlayer;

        private AudioSource _audioSource;

        private Tween _tweener;

        private ObservationPointPositionInfo _observationPointPositionInfo;
        private int _observationPointPositionInfoIndex;

        private void Awake()
        {
            // 初始化玩家 Transform，可以通过查找特定的游戏对象或通过其他方式获取
            if (Camera.main != null) _playerTransform = Camera.main.transform;
            _videoPlayer = GetComponent<VideoPlayer>();
            _audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            LookAtPlayer();

            if (Input.GetKeyDown(KeyCode.F7))
            {
                NextIntroduceChart();
            }
            else if (Input.GetKeyDown(KeyCode.F8))
            {
                PreviousIntroduceChart();
            }
        }

        /// <summary>
        /// 显示介绍图片，设置画布位置
        /// </summary>
        /// <param name="observedObject">被观测目标</param>
        public void SetToTargetPositionAndInfo(ObservedObject observedObject)
        {
            transform.position = observedObject.transform.position;
            _observationPointPositionInfo = Configuration.Instance.configList.First(info => info.observationPoint == observedObject.transform);
            _observationPointPositionInfoIndex = 0;
            PlayResource(_observationPointPositionInfo.introductionChart[_observationPointPositionInfoIndex]);
        }

        /// <summary>
        /// 切换到下一张介绍图片
        /// </summary>
        public void NextIntroduceChart()
        {
            if (_observationPointPositionInfo == null || _observationPointPositionInfo.introductionChart.Count <= 1) return;
            // 更新索引并防止越界
            _observationPointPositionInfoIndex = (_observationPointPositionInfoIndex + 1) % _observationPointPositionInfo.introductionChart.Count;
            // 更新显示的图片
            PlayResource(_observationPointPositionInfo.introductionChart[_observationPointPositionInfoIndex]);
        }

        /// <summary>
        /// 切换到上一张介绍图片
        /// </summary>
        public void PreviousIntroduceChart()
        {
            if (_observationPointPositionInfo == null || _observationPointPositionInfo.introductionChart.Count <= 1) return;
            // 更新索引并防止越界
            _observationPointPositionInfoIndex = (_observationPointPositionInfoIndex - 1 + _observationPointPositionInfo.introductionChart.Count) % _observationPointPositionInfo.introductionChart.Count;
            // 更新显示的图片
            PlayResource(_observationPointPositionInfo.introductionChart[_observationPointPositionInfoIndex]);
        }

        public void ShowAnim()
        {
            _tweener?.Kill();
            transform.localScale = Vector3.zero;
            _tweener = transform.DOScale(FixedScreenSizeObject.Instance.GetScale(), 0.5f);
        }

        public void HideAnim()
        {
            _tweener?.Kill();
            _tweener = transform.DOScale(0, 0.5f);
            _videoPlayer.Stop();
            _audioSource.Stop();
        }

        private void PlayResource(IntroductionMedia introductionMedia)
        {
            // 清除之前的图像或视频内容
            showIntroduceImg.texture = null;
            if (_videoPlayer.clip)
                _videoPlayer.Stop();

            switch (introductionMedia.mediaType)
            {
                case IntroductionMedia.MediaType.Image:
                    // 显示图片
                    var imageTexture = introductionMedia.mediaContent as Texture;
                    if (imageTexture)
                    {
                        showIntroduceImg.texture = imageTexture;
                        resourceIconType.texture = resourceImgIcon;
                    }

                    break;

                case IntroductionMedia.MediaType.Video:
                    // 播放视频
                    var videoClip = introductionMedia.mediaContent as VideoClip;

                    resourceIconType.texture = resourceVideoIcon;

                    if (videoClip)
                    {
                        // 设置视频源
                        _videoPlayer.clip = videoClip;

                        // 创建 RenderTexture 用于渲染视频画面
                        var renderTexture = new RenderTexture((int)videoClip.width, (int)videoClip.height, 24);
                        _videoPlayer.targetTexture = renderTexture;

                        // 将 RenderTexture 分配给 RawImage
                        var oldTex = showIntroduceImg.texture;
                        showIntroduceImg.texture = renderTexture;

                        // 如果之前有 RenderTexture，释放旧资源避免内存泄漏
                        if (oldTex is RenderTexture oldRT)
                        {
                            oldRT.Release();
                            Destroy(oldRT);
                        }

                        // 开始播放视频
                        _videoPlayer.Play();
                    }

                    break;
            }

            if (introductionMedia.audioClip)
            {
                _audioSource.clip = introductionMedia.audioClip;
                _audioSource.Play();
            }
        }

        /// <summary>
        /// 持续看向玩家，UI需要旋转180度
        /// </summary>
        private void LookAtPlayer()
        {
            if (!_playerTransform) return;

            // 使用 LookAt 方法使 Canvas 面向玩家
            transform.LookAt(_playerTransform);

            // 因为需要反向面对玩家，所以绕 Y 轴旋转 180 度
            transform.Rotate(0, 180f, 0);
        }
    }
}
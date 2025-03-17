using System;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace JD.FPS
{
    // TODO: Add public property for inspectors, etc.
    public class FPSMonitor : MonoBehaviour
    {
        public const int ResetStatsFPS = 200;
        public const int Good = 58;
        public const int Bad = 32;
        
        public Image FPS;
        public Image Allocated;
        public Image Mono;
        public Image Reserved;

        public TextMeshProUGUI FPSValue;
        public TextMeshProUGUI AverageValue;
        public TextMeshProUGUI MinValue;
        public TextMeshProUGUI MaxValue;
        
        public TextMeshProUGUI AllocatedValue;
        public TextMeshProUGUI MonoValue;
        public TextMeshProUGUI ReservedValue;

        private Color _goodColor = new Color(118 / 255f, 212 / 255f, 58 / 255f);
        private Color _okColor = new Color(243 / 255f, 232 / 255f, 0 / 255f);
        private Color _badColor = new Color(220 / 255f, 41 / 255f, 30 / 255f);
        
        private Color _allocatedColor = new Color(255 / 255f, 190 / 255f, 60 / 255f);
        private Color _monoColor = new Color(76 / 255f, 166 / 255f, 255 / 255f);
        private Color _reservedColor = new Color(205 / 255f, 84 / 255f, 229 / 255f);
        
        private Graph _fpsGraph;
        private Graph _allocatedGraph;
        private Graph _monoGraph;
        private Graph _reservedGraph;

        private float _minFPS = 60;
        private float _maxFPS = 60;
        
        private float _averageFPS = 60f;
        private float _averageTotal = 60f;
        private int _averageCount = 1;
        
        private float _resetStatsFPS = 1;

        private int _lastFPSValue = -1;
        private int _lastAverageValue = -1;
        private int _lastMinValue = -1;
        private int _lastMaxValue = -1;
        
        private int _lastAllocatedValue = -1;
        private int _lastMonoValue = -1;
        private int _lastReservedValue = -1;

        private float _allocated;
        private float _mono;
        private float _reserved;

        private bool _firstTime;
        
        private void Start()
        {
            _fpsGraph = new Graph(FPS);
            _allocatedGraph = new Graph(Allocated);
            _monoGraph = new Graph(Mono);
            _reservedGraph = new Graph(Reserved);
            
            _fpsGraph.SetColors(_goodColor, _okColor, _badColor);
            
            _fpsGraph.SetThresholds(Good, Bad);
        }

        private void OnEnable()
        {
            _resetStatsFPS = 1;
            _firstTime = true;
        }

        private void Update()
        {
            // RAM
            _allocated = Profiler.GetTotalAllocatedMemoryLong() / 1048576f;
            _mono = Profiler.GetMonoUsedSizeLong() / 1048576f;
            _reserved = Profiler.GetTotalReservedMemoryLong() / 1048576f;

            var maxRam = 0f;
            if (_allocated > maxRam) maxRam = _allocated;
            if (_mono > maxRam) maxRam = _mono;
            if (_reserved > maxRam) maxRam = _reserved;

            maxRam *= 1.25f;
            
            _allocatedGraph.Add(_allocated, maxRam);
            _monoGraph.Add(_mono, maxRam);
            _reservedGraph.Add(_reserved, maxRam);
            
            // FPS
            var fps = (int) (1 / Time.unscaledDeltaTime);
            
            if (fps < 0) fps = 0;
            else if (fps > 1000) fps = 1000;

            if (--_resetStatsFPS == 0)
            {
                _resetStatsFPS = ResetStatsFPS;

                _averageFPS = _averageTotal / _averageCount;

                _averageCount = 1;
                _averageTotal = fps;
                
                // Update Texts only every 1 sec. No need for every frame and was consuming quite a lot of CPU...
                
                // Update FPS
                // These produce no gc alloc (but will show alloc in editor mode)
                if (_lastFPSValue != (int) _averageFPS) FPSValue.SetText("{0}", (int) _averageFPS); //fps);
                if (_lastAverageValue != (int) _averageFPS) AverageValue.SetText("{0}", (int) _averageFPS);
                if (_lastMinValue != (int) _minFPS) MinValue.SetText("{0}", (int) _minFPS);
                if (_lastMaxValue != (int) _maxFPS) MaxValue.SetText("{0}", (int) _maxFPS);

                AverageValue.color = _averageFPS > Good ? _goodColor : _averageFPS > Bad ? _okColor : _badColor;
                MinValue.color = _minFPS > Good ? _goodColor : _minFPS > Bad ? _okColor : _badColor;
                MaxValue.color = _maxFPS > Good ? _goodColor : _maxFPS > Bad ? _okColor : _badColor;
                
                _lastFPSValue = (int) _averageFPS;
                _lastAverageValue = (int) _averageFPS;
                _lastMinValue = (int) _minFPS;
                _lastMaxValue = (int) _maxFPS;

                // Update RAM
                // No alloc in build
                if (_lastAllocatedValue != (int) (_allocated * 100)) AllocatedValue.SetText("{0:2} A", _allocated);
                if (_lastMonoValue != (int) (_mono * 100)) MonoValue.SetText("{0:2} M", _mono);
                if (_lastReservedValue != (int) (_reserved * 100)) ReservedValue.SetText("{0:2} R", _reserved);
                
                _lastAllocatedValue = (int) (_allocated * 100);
                _lastMonoValue = (int) (_mono * 100);
                _lastReservedValue = (int) (_reserved * 100);
                
                AllocatedValue.color = _allocatedColor;
                MonoValue.color = _monoColor;
                ReservedValue.color = _reservedColor;
                
                // Reset
                _maxFPS = 0;
                _minFPS = float.MaxValue;
                _maxFPS = 0;
                
                if (_firstTime)
                {
                    _firstTime = false;
                    fps = 60;
                }
            }
            else
            {
                _averageCount++;
                _averageTotal += fps;
            }
            
            if (fps < _minFPS)
                _minFPS = fps;
            
            if (fps > _maxFPS)
                _maxFPS = fps;

            _fpsGraph.SetAverage(_averageFPS);
            _fpsGraph.Add(fps, _maxFPS * 1.20f);
        }

        private void OnDestroy()
        {
            _fpsGraph.Reset();
        }
    }
}
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace jd.boivin.fps
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

        private float _minFPS;
        private float _maxFPS;
        
        private float _averageFPS = 60f;
        private float _averageTotal = 0f;
        private int _averageCount = 1;
        
        private float _resetStatsFPS = ResetStatsFPS;
        
        private void Start()
        {
            _fpsGraph = new Graph(FPS);
            _allocatedGraph = new Graph(Allocated);
            _monoGraph = new Graph(Mono);
            _reservedGraph = new Graph(Reserved);
            
            _fpsGraph.SetColors(_goodColor, _okColor, _badColor);
            
            _fpsGraph.SetThresholds(Good, Bad);
        }

        private void Update()
        {
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
                
                _maxFPS = 0;
                _minFPS = float.MaxValue;
                _maxFPS = 0;
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

            // These produce no gc alloc (but will show alloc in editor mode)
            FPSValue.SetText("{0}", (int) fps);
            AverageValue.SetText("{0}", (int) _averageFPS);
            MinValue.SetText("{0}", (int) _minFPS);
            MaxValue.SetText("{0}", (int) _maxFPS);

            AverageValue.color = _averageFPS > Good ? _goodColor : _averageFPS > Bad ? _okColor : _badColor;
            MinValue.color = _minFPS > Good ? _goodColor : _minFPS > Bad ? _okColor : _badColor;
            MaxValue.color = _maxFPS > Good ? _goodColor : _maxFPS > Bad ? _okColor : _badColor;

            // RAM
            var allocated = Profiler.GetTotalAllocatedMemoryLong() / 1048576f;
            var mono = Profiler.GetMonoUsedSizeLong() / 1048576f;
            var reserved = Profiler.GetTotalReservedMemoryLong() / 1048576f;

            var maxRam = 0f;
            if (allocated > maxRam) maxRam = allocated;
            if (mono > maxRam) maxRam = mono;
            if (reserved > maxRam) maxRam = reserved;

            maxRam *= 1.25f;
            
            _allocatedGraph.Add(allocated, maxRam);
            _monoGraph.Add(mono, maxRam);
            _reservedGraph.Add(reserved, maxRam);
            
            // No alloc in build
            AllocatedValue.SetText("{0:2} A", allocated);
            MonoValue.SetText("{0:2} M", mono);
            ReservedValue.SetText("{0:2} R", reserved);

            AllocatedValue.color = _allocatedColor;
            MonoValue.color = _monoColor;
            ReservedValue.color = _reservedColor;
        }

        private void OnDestroy()
        {
            _fpsGraph.Reset();
        }
    }
}
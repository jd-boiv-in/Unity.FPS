using UnityEngine;
using UnityEngine.UI;

namespace JD.FPS
{
    public class Graph
    {
        private const int MaxValues = 128;
        
        private static readonly int GraphValues = Shader.PropertyToID("GraphValues");
        private static readonly int GraphValuesLength = Shader.PropertyToID("GraphValues_Length");
        private static readonly int Average = Shader.PropertyToID("Average");
        private static readonly int GoodThreshold = Shader.PropertyToID("_GoodThreshold");
        private static readonly int CautionThreshold = Shader.PropertyToID("_CautionThreshold");
        
        private static readonly int GoodColor = Shader.PropertyToID("_GoodColor");
        private static readonly int CautionColor = Shader.PropertyToID("_CautionColor");
        private static readonly int CriticalColor = Shader.PropertyToID("_CriticalColor");

        private readonly float[] _values = new float[MaxValues];
        private readonly float[] _points = new float[MaxValues];
        
        private readonly Image _image;

        private float _good = 0f;
        private float _bad = 0f;
        
        private float _average = 0f;
        
        public Graph(Image image)
        {
            _image = image;
                
            _image.material.SetFloatArray(GraphValues, _points);
            _image.material.SetInt(GraphValuesLength, MaxValues);
        }

        public void SetThresholds(float good, float bad)
        {
            _good = good;
            _bad = bad;
        }

        public void SetColors(Color good, Color caution, Color critical)
        {
            _image.material.SetColor(GoodColor, good);
            _image.material.SetColor(CautionColor, caution);
            _image.material.SetColor(CriticalColor, critical);
        }

        public void SetAverage(float average)
        {
            _average = average;
        }
        
        public void Add(float add, float highest = 0)
        {
            for (var i = 0; i < MaxValues; i++)
            {
                if (i == MaxValues - 1)
                    _values[i] = add;
                else
                    _values[i] = _values[i + 1];

                if (_values[i] > highest) highest = _values[i];
            }
            
            for (var i = 0; i < MaxValues; i++)
            {
                _points[i] = _values[i] / highest;
            }

            _image.material.SetFloatArray(GraphValues, _points);
            _image.material.SetInt(GraphValuesLength, MaxValues);
            
            _image.material.SetFloat(GoodThreshold, _good / highest);
            _image.material.SetFloat(CautionThreshold, _bad / highest);
            
            _image.material.SetFloat(Average, _average / highest);
        }

        public void Reset()
        {
            _image.material.SetFloat(CautionThreshold, 0);
            _image.material.SetFloat(GoodThreshold, 0);
        }
    }
}
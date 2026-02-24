using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DigitalTwin.Services.ML;

/// <summary>
/// ONNX Runtime ile offline model çalıştırma (opsiyonel, gelecek özellik)
/// Model: Kullanıcı davranış örüntüsü tahmin modeli
/// </summary>
public class OnnxPredictionService
{
    private InferenceSession? _session;
    private readonly string _modelPath;

    public OnnxPredictionService()
    {
        _modelPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Models",
            "behavior_model.onnx"
        );
    }

    public void LoadModel()
    {
        if (!File.Exists(_modelPath))
        {
            throw new FileNotFoundException($"ONNX model not found: {_modelPath}");
        }

        _session = new InferenceSession(_modelPath);
    }

    /// <summary>
    /// Örnek: Yarınki üretkenlik tahmini
    /// Input: Son 7 günün özet verileri
    /// Output: Tahmin edilen verimlilik skoru (0-100)
    /// </summary>
    public float PredictProductivity(float[] last7DaysScores)
    {
        if (_session == null)
            throw new InvalidOperationException("Model not loaded");

        // Prepare input tensor
        var inputTensor = new DenseTensor<float>(last7DaysScores, new[] { 1, 7 });
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input", inputTensor)
        };

        // Run inference
        using var results = _session.Run(inputs);
        var output = results.First().AsEnumerable<float>().First();

        return output;
    }

    /// <summary>
    /// Örnek: Dikkat dağıtma riski tahmini
    /// Input: Mevcut saat, günün saati, son aktiviteler
    /// Output: Risk skoru (0-1)
    /// </summary>
    public float PredictDistractionRisk(int hourOfDay, float recentProductivity)
    {
        if (_session == null)
            throw new InvalidOperationException("Model not loaded");

        // Simplified example
        var input = new float[] { hourOfDay / 24f, recentProductivity };
        var inputTensor = new DenseTensor<float>(input, new[] { 1, 2 });
        
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input", inputTensor)
        };

        using var results = _session.Run(inputs);
        return results.First().AsEnumerable<float>().First();
    }

    public void Dispose()
    {
        _session?.Dispose();
    }
}

/* 
 * NOT: ONNX model eğitimi için Python örneği:
 * 
 * import numpy as np
 * import onnx
 * from sklearn.ensemble import RandomForestRegressor
 * from skl2onnx import convert_sklearn
 * from skl2onnx.common.data_types import FloatTensorType
 * 
 * # Train model
 * X = np.random.rand(100, 7)  # 7 günlük veri
 * y = np.random.rand(100)     # Verimlilik skoru
 * model = RandomForestRegressor()
 * model.fit(X, y)
 * 
 * # Convert to ONNX
 * initial_type = [('input', FloatTensorType([None, 7]))]
 * onnx_model = convert_sklearn(model, initial_types=initial_type)
 * 
 * # Save
 * with open("behavior_model.onnx", "wb") as f:
 *     f.write(onnx_model.SerializeToString())
 */

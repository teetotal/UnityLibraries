using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
public class Barracuda
{
    private Model mRuntimeModel;
    private string mInputName, mOutputName;
    IWorker mWorker = null;
    
    private static readonly Lazy<Barracuda> hInstance =
        new Lazy<Barracuda>(() => new Barracuda());
 
    public static Barracuda Instance
    {
        get {
            return hInstance.Value;
        } 
    }

    protected Barracuda()
    {
    }
    public void LoadModel(NNModel model)
    {
        mRuntimeModel = ModelLoader.Load(model);
        mWorker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, mRuntimeModel);
        Debug.Log("model loaded");
    }
    public bool IsLoaded()
    {
        if(mWorker == null)
            return false;

        return true;
    }
    /*
    private void CreateTensor()
    {
        float[] buffer = Enumerable.Repeat<float>(-1, size_node + (size_node * size_positions * 2)).ToArray<float>();
    }
    */
    public List<float> Execute_Shape1(List<float[]> tensor, int shape)
    {
        float[][] inputBuffer = tensor.ToArray();
        var inputs = new Dictionary<string, Tensor>();
        inputs[mRuntimeModel.inputs[0].name] = new Tensor(tensor.Count, shape, inputBuffer);
        

        return Execute(inputs);
    }
    private List<float> Execute(Dictionary<string, Tensor> inputs)
    {        
        /*
        mInputName = mRuntimeModel.inputs[0].name;
        mOutputName = mRuntimeModel.outputs[0];
        */
        var output = mWorker.Execute(inputs).PeekOutput(mRuntimeModel.outputs[0]);
        List<float> ret = new List<float>();
        for (int n = 0; n < output.length; n++)
        {
            //Debug.Log(output[n].ToString());
            ret.Add(output[n]);
        }
        inputs[mRuntimeModel.inputs[0].name].Dispose();
        output.Dispose();

        return ret;
    }

    public List<float> Execute_Shape3(float[] tensor, int[] shape)
    {
        var inputs = new Dictionary<string, Tensor>();
        inputs[mRuntimeModel.inputs[0].name] = new Tensor(1, shape[0], shape[1], shape[2], tensor);
        /*
        mInputName = mRuntimeModel.inputs[0].name;
        mOutputName = mRuntimeModel.outputs[0];
        */
        var output = mWorker.Execute(inputs).PeekOutput(mRuntimeModel.outputs[0]);
        List<float> ret = new List<float>();
        for(int n = 0; n<output.length; n++)
        {
            //Debug.Log(output[n].ToString());
            ret.Add(output[n]);
        }
        inputs[mRuntimeModel.inputs[0].name].Dispose();
        output.Dispose();

        return ret;
    }
    public List<float> Execute_Shape3(List<float[]> tensor, int[] shape)
    {
        var inputs = new Dictionary<string, Tensor>();
        /*
        float[] inputBuffer = new float[tensor[0].Length * tensor.Count];
        for(int n=0; n < tensor.Count; n++)
        {
            tensor[n].CopyTo(inputBuffer, n * tensor[0].Length);
        }
        */
        float[][] inputBuffer = tensor.ToArray();
        
        inputs[mRuntimeModel.inputs[0].name] = new Tensor(tensor.Count, shape[0], shape[1], shape[2], inputBuffer);
        /*
        mInputName = mRuntimeModel.inputs[0].name;
        mOutputName = mRuntimeModel.outputs[0];
        */
        var output = mWorker.Execute(inputs).PeekOutput(mRuntimeModel.outputs[0]);
        List<float> ret = new List<float>();
        for(int n = 0; n<output.length; n++)
        {
            //Debug.Log(output[n].ToString());
            ret.Add(output[n]);
        }
        inputs[mRuntimeModel.inputs[0].name].Dispose();
        output.Dispose();

        return ret;
    }
}
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork
{

    private CellBlock[] cellBlocks;

    public NeuralNetwork(int blocks, int inputSize, int outputSize, float wiegthRange, float biasRange, float learningRate, int timeSteps, int traningCyckels)
    {

        cellBlocks = new CellBlock[blocks];
        for (int i = 0; i < blocks; i++)
        {
            cellBlocks[i] = new CellBlock(inputSize, outputSize, wiegthRange, biasRange, learningRate, timeSteps, traningCyckels);
        }
    }

    public float[] CalculateOutput(float[] inputs)
    {

        float[][] temp = cellBlocks[0].ProccessCell(inputs);
        float[] result = new float[temp.Length];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = temp[i][0];
        }

        return result;
    }

    public void BackProp(float[][][] recInputs, float[][][] expectOut) {
        cellBlocks[0].BackProp(recInputs, expectOut);
        Debug.Log("BackpropDone");
    }

}

public class CellBlock
{

    //output
    private float[][] h_prev;
    private float[][] ht;

    //cell state
    private float[][] c_prev;
    private float[][] ct;

    //input wieghts
    private float[][] wf;
    private float[][] wo;
    private float[][] wi;
    private float[][] wc;

    //last value wieghts
    private float[][] uf;
    private float[][] uo;
    private float[][] ui;
    private float[][] uc;

    //bias
    private float[][] bf;
    private float[][] bo;
    private float[][] bi;
    private float[][] bc;

    //traning variables
    private float[][][][] backpropValues;
    private float[][][][] gates;
    private float[][][] W;
    private float[][][] U;
    private float[][][] B;

    private int inputSize;
    private int outputSize;
    private int timeSteps;
    private int timeStep;
    private int traningCyckels;
    private float learningRate;

    public CellBlock(int inputSize, int outputSize, float wieghtRange, float biasRange, float learningRate, int timeSteps, int traningCyckels)
    {
        this.inputSize = inputSize;
        this.outputSize = outputSize;
        this.timeSteps = timeSteps;
        this.traningCyckels = traningCyckels;
        this.timeStep = timeSteps;
        this.learningRate = learningRate;

        //initialize output vector to value 0;
        float[] hTemp = new float[outputSize];
        for (int i = 0; i < outputSize; i++)
            hTemp[i] = 0;

        //wieghts for input vector
        float[] wTemp = new float[outputSize * inputSize];
        for (int i = 0; i < outputSize * inputSize; i++)
            wTemp[i] = Random.Range(-wieghtRange, wieghtRange);

        //wiegts for cellstate and output
        float[] uTemp = new float[outputSize * outputSize];
        for (int i = 0; i < outputSize * outputSize; i++)
            uTemp[i] = Random.Range(-wieghtRange, wieghtRange);

        //bias values
        float[] bTemp = new float[outputSize];
        for (int i = 0; i < outputSize; i++)
            bTemp[i] = Random.Range(-biasRange, biasRange);

        //set previus state to zero
        float[][] zeroMatrix = ConvertArrayToMatrix(hTemp, outputSize, 1);
        h_prev = CopyMatrix(zeroMatrix);
        c_prev = CopyMatrix(zeroMatrix);

        //initialize input wieghts to random values
        float[][] w = ConvertArrayToMatrix(wTemp, outputSize, inputSize);
        wf = CopyMatrix(w);
        wo = CopyMatrix(w);
        wi = CopyMatrix(w);
        wc = CopyMatrix(w);

        //initialize output and cell wigths to random values
        float[][] u = ConvertArrayToMatrix(uTemp, outputSize, outputSize);
        uf = CopyMatrix(u);
        uo = CopyMatrix(u);
        ui = CopyMatrix(u);
        uc = CopyMatrix(u);

        //initialize bias values
        float[][] b = ConvertArrayToMatrix(bTemp, outputSize, 1);
        bf = CopyMatrix(b);
        bo = CopyMatrix(b);
        bi = CopyMatrix(b);
        bc = CopyMatrix(b);

        //initialize storing values
        float[] tempInput = new float[inputSize];
        float[][] tempXT = ConvertArrayToMatrix(tempInput, inputSize, 1);
        backpropValues = new float[timeSteps][][][];
        for (int i = 0; i < timeSteps; i++)
        {
            backpropValues[i] = new float[3][][];
            backpropValues[i][0] = ConvertArrayToMatrix(tempInput, inputSize, 1);  //inputs
            backpropValues[i][1] = CopyMatrix(zeroMatrix);                         //outputs
            backpropValues[i][2] = CopyMatrix(zeroMatrix);                         //expected outputs
        }

        gates = new float[timeSteps][][][];
        for (int i = 0; i < timeSteps; i++)
        {
            gates[i] = new float[5][][];
            gates[i][0] = ElementWiseTanh(MatrixAddition(MatrixMultiplication(wc, CopyMatrix(tempXT)), MatrixMultiplication(uc, h_prev), bc));              //a gate                                                                                                      //cellstate
            gates[i][1] = ElementWiseLogSigmoid(MatrixAddition(MatrixMultiplication(wi, CopyMatrix(tempXT)), MatrixMultiplication(ui, h_prev), bi));       //input gate
            gates[i][2] = ElementWiseLogSigmoid(MatrixAddition(MatrixMultiplication(wf, CopyMatrix(tempXT)), MatrixMultiplication(uf, h_prev), bf));       //forget gate
            gates[i][3] = ElementWiseLogSigmoid(MatrixAddition(MatrixMultiplication(wo, CopyMatrix(tempXT)), MatrixMultiplication(uo, h_prev), bo));       //output gate
            gates[i][4] = CopyMatrix(zeroMatrix);                                                                                                          //cell state
        }

        W = new float[4][][];
        W[0] = CopyMatrix(wc);       //W for cell
        W[1] = CopyMatrix(wi);       //W for input
        W[2] = CopyMatrix(wf);       //W for forget gate
        W[3] = CopyMatrix(wo);       //W for output

        U = new float[4][][];
        U[0] = CopyMatrix(uc);       //U for cell
        U[1] = CopyMatrix(ui);       //U for input
        U[2] = CopyMatrix(uf);       //U for forget gate
        U[3] = CopyMatrix(uo);       //U for output

        B = new float[4][][];
        B[0] = CopyMatrix(bc);
        B[1] = CopyMatrix(bi);
        B[2] = CopyMatrix(bf);
        B[3] = CopyMatrix(bo);
    }

    public float[][] ProccessCell(float[] input)
    {
        float[][] xt = ConvertArrayToMatrix(input, inputSize, 1);
        float[][] ft = ElementWiseLogSigmoid(MatrixAddition(MatrixMultiplication(wf, xt), MatrixMultiplication(uf, h_prev), bf));
        float[][] it = ElementWiseLogSigmoid(MatrixAddition(MatrixMultiplication(wi, xt), MatrixMultiplication(ui, h_prev), bi));
        float[][] ot = ElementWiseLogSigmoid(MatrixAddition(MatrixMultiplication(wo, xt), MatrixMultiplication(uo, h_prev), bo));
        float[][] at = ElementWiseTanh(MatrixAddition(MatrixMultiplication(wc, xt), MatrixMultiplication(uc, h_prev), bc));
        ct = MatrixAddition(ElementWiseMultiplication(ft, c_prev), ElementWiseMultiplication(it, at));
        ht = ElementWiseMultiplication(ot, ElementWiseTanh(ct));
        c_prev = CopyMatrix(ct);
        h_prev = CopyMatrix(ht);

        //DebugTestMatrixMultiplication();

        return ht;
    }

    public float[][] ProccessCellBackProp(float[] input, float[] expectedOutput, int timeStep)
    {
        float[][] xt = ConvertArrayToMatrix(input, inputSize, 1);
        float[][] ft = ElementWiseLogSigmoid(MatrixAddition(MatrixMultiplication(wf, xt), MatrixMultiplication(uf, h_prev), bf));
        float[][] it = ElementWiseLogSigmoid(MatrixAddition(MatrixMultiplication(wi, xt), MatrixMultiplication(ui, h_prev), bi));
        float[][] ot = ElementWiseLogSigmoid(MatrixAddition(MatrixMultiplication(wo, xt), MatrixMultiplication(uo, h_prev), bo));
        float[][] at = ElementWiseTanh(MatrixAddition(MatrixMultiplication(wc, xt), MatrixMultiplication(uc, h_prev), bc));
        ct = MatrixAddition(ElementWiseMultiplication(ft, c_prev), ElementWiseMultiplication(it, at));
        ht = ElementWiseMultiplication(ot, ElementWiseTanh(ct));
        c_prev = CopyMatrix(ct);
        h_prev = CopyMatrix(ht);

        gates[timeStep][0] = CopyMatrix(at);
        gates[timeStep][1] = CopyMatrix(it);
        gates[timeStep][2] = CopyMatrix(ft);
        gates[timeStep][3] = CopyMatrix(ot);
        gates[timeStep][4] = CopyMatrix(ct);

        backpropValues[timeStep][0] = CopyMatrix(xt);
        backpropValues[timeStep][1] = CopyMatrix(ht);
        backpropValues[timeStep][2] = ConvertArrayToMatrix(expectedOutput, outputSize, 1);

        return ht;
    }

    public void StartBackporpagation(float[][] recordedValues)
    {

        int pointer = recordedValues.Length / traningCyckels;

        float[][][] inputs = new float[traningCyckels][][];
        for (int i = 0; i < traningCyckels; i++)
        {
            inputs[i] = new float[timeStep][];
        }

        float[][][] expectOut = new float[traningCyckels][][];
        for (int i = 0; i < traningCyckels; i++)
        {
            expectOut[i] = new float[timeStep][];
        }

        for (int y = 1; y <= traningCyckels; y++)
        {
            for (int i = (y - 1) * pointer; i < y * pointer; i++)
            {
                inputs[y][i] = recordedValues[0];
                expectOut[y][i] = recordedValues[1];
            }
        }

        ResetBacpropValues();

        BackProp(inputs, expectOut);
    }

    private void ResetBacpropValues()
    {
        float[] hTemp = new float[outputSize];
        for (int i = 0; i < outputSize; i++)
            hTemp[i] = 0;

        //wieghts for input vector
        float[] wTemp = new float[outputSize * inputSize];
        for (int i = 0; i < outputSize * inputSize; i++)
            wTemp[i] = 0;

        //wiegts for cellstate and output
        float[] uTemp = new float[outputSize * outputSize];
        for (int i = 0; i < outputSize * outputSize; i++)
            uTemp[i] = 0;

        //bias values
        float[] bTemp = new float[outputSize];
        for (int i = 0; i < outputSize; i++)
            bTemp[i] = 0;

        float[] tempInput = new float[inputSize];
        float[][] w = ConvertArrayToMatrix(wTemp, outputSize, inputSize);
        float[][] u = ConvertArrayToMatrix(uTemp, outputSize, outputSize);
        float[][] b = ConvertArrayToMatrix(bTemp, outputSize, 1);
        float[][] zeroMatrix = ConvertArrayToMatrix(hTemp, outputSize, 1);
        float[][] tempXT = ConvertArrayToMatrix(tempInput, inputSize, 1);

        backpropValues = new float[timeSteps][][][];
        for (int i = 0; i < timeSteps; i++)
        {
            backpropValues[i] = new float[3][][];
            backpropValues[i][0] = ConvertArrayToMatrix(tempInput, inputSize, 1);  //inputs
            backpropValues[i][1] = CopyMatrix(zeroMatrix);                         //outputs
            backpropValues[i][2] = CopyMatrix(zeroMatrix);                         //expected outputs
        }

        gates = new float[timeSteps][][][];
        for (int i = 0; i < timeSteps; i++)
        {
            gates[i] = new float[4][][];
            gates[i][0] = ElementWiseTanh(MatrixAddition(MatrixMultiplication(wc, CopyMatrix(tempXT)), MatrixMultiplication(CopyMatrix(u), CopyMatrix(tempXT)), CopyMatrix(b)));                        //a state
            gates[i][1] = ElementWiseLogSigmoid(MatrixAddition(MatrixMultiplication(CopyMatrix(w), CopyMatrix(tempXT)), MatrixMultiplication(CopyMatrix(u), CopyMatrix(tempXT)), CopyMatrix(b)));       //input gate
            gates[i][2] = ElementWiseLogSigmoid(MatrixAddition(MatrixMultiplication(CopyMatrix(w), CopyMatrix(tempXT)), MatrixMultiplication(CopyMatrix(u), CopyMatrix(tempXT)), CopyMatrix(b)));       //forget gate
            gates[i][3] = ElementWiseLogSigmoid(MatrixAddition(MatrixMultiplication(CopyMatrix(w), CopyMatrix(tempXT)), MatrixMultiplication(CopyMatrix(u), CopyMatrix(tempXT)), CopyMatrix(b)));       //output gate
            gates[i][4] = CopyMatrix(zeroMatrix);                                                                                                                                                       //cell state
        }

        W = new float[4][][];
        W[0] = CopyMatrix(wc);       //W for cell
        W[1] = CopyMatrix(wi);       //W for input
        W[2] = CopyMatrix(wf);       //W for forget gate
        W[3] = CopyMatrix(wo);       //W for output

        U = new float[4][][];
        U[0] = CopyMatrix(uc);       //U for cell
        U[1] = CopyMatrix(ui);       //U for input
        U[2] = CopyMatrix(uf);       //U for forget gate
        U[3] = CopyMatrix(uo);       //U for output

        B = new float[4][][];
        B[0] = CopyMatrix(bc);
        B[1] = CopyMatrix(bi);
        B[2] = CopyMatrix(bf);
        B[3] = CopyMatrix(bo);
    }

    public void BackProp(float[][][] recInputs, float[][][] expectOut)
    {
        for (int cycle = 0; cycle < traningCyckels; cycle++)
        {

            for (int i = 0; i < timeStep; i++)
            {
                //process x frames of data and store in variables
                ProccessCellBackProp(recInputs[cycle][i], expectOut[cycle][i], i);
            }

            float[][] gradOut = new float[0][];
            float[][] gradState = new float[0][];
            float[][] gradA = new float[0][];
            float[][] gradInput = new float[0][];
            float[][] gradForget = new float[0][];
            float[][] gradOuput = new float[0][];
            float[][][] gradOutNext = new float[timeStep][][];
            float[][][] gradX = new float[timeStep][][];
            float[][][][] gradGates = new float[timeStep][][][];

            for (int i = 0; i < timeStep; i++) {
                gradGates[i] = new float[4][][];
            }


            for (int currentTimeStep = timeSteps - 1; currentTimeStep > 0; currentTimeStep--)
            {

                if (currentTimeStep + 1 < timeSteps)
                {
                    //calculate gradient for output
                    gradOut = MatrixAddition(ElementalWiseSubtraction(backpropValues[currentTimeStep][1], backpropValues[currentTimeStep][2]), backpropValues[currentTimeStep + 1][1]);

                    //calculate gradient for state
                    gradState = MatrixAddition(ElementWiseMultiplication(ElementWiseMultiplication(gradOut, gates[currentTimeStep][3]), ElementWiseTanh2(gates[currentTimeStep][0])),
                                               ElementWiseMultiplication(gates[currentTimeStep + 1][0], gates[currentTimeStep + 1][2]));

                }
                else
                {
                    gradOut = ElementalWiseSubtraction(backpropValues[currentTimeStep][1], backpropValues[currentTimeStep][2]);
                    gradState = ElementWiseMultiplication(ElementWiseMultiplication(gradOut, gates[currentTimeStep][3]), ElementWiseTanh2(gates[currentTimeStep][0]));

                }

                // a = 0, i = 1, f = 2, o = 3 

                gradA = ElementWiseMultiplication(ElementWiseMultiplication(gradState, gates[currentTimeStep][1]), ElementWiseTanh2(gates[currentTimeStep][0]));
                gradInput = ElementWiseMultiplication(ElementWiseMultiplication(ElementWiseMultiplication(gradState, gates[currentTimeStep][0]), gates[currentTimeStep][1]), MinusOneInvers(gates[currentTimeStep][1]));

                if (currentTimeStep - 1 > 0)
                {
                    gradForget = ElementWiseMultiplication(ElementWiseMultiplication(ElementWiseMultiplication(gradState, gates[currentTimeStep - 1][4]), gates[currentTimeStep][2]), MinusOneInvers(gates[currentTimeStep][2]));
                }
                else
                {
                    gradForget = ElementWiseMultiplication(ElementWiseMultiplication(ElementWiseMultiplication(gradState, ZeroMatrix(gates[currentTimeStep][4])), gates[currentTimeStep][2]), MinusOneInvers(gates[currentTimeStep][2]));
                }

                gradOuput = ElementWiseMultiplication(ElementWiseMultiplication(ElementWiseMultiplication(gradOut, ElementWiseTanh(gates[currentTimeStep][4])), gates[currentTimeStep][3]), MinusOneInvers(gates[currentTimeStep][3]));

                gradGates[currentTimeStep][0] = gradA;
                gradGates[currentTimeStep][1] = gradInput;
                gradGates[currentTimeStep][2] = gradForget;
                gradGates[currentTimeStep][3] = gradOuput;

                //float[][] aMult = MatrixMultiplication(W[0], gradGates[currentTimeStep][0]);
                //float[][] inMult = MatrixMultiplication(W[1], gradGates[currentTimeStep][1]);
                //float[][] forgetMult = MatrixMultiplication(W[2], gradGates[currentTimeStep][2]);
                //float[][] outMult = MatrixMultiplication(W[3], gradGates[currentTimeStep][3]);

                gradX[currentTimeStep] = MatrixMultiplication(ConvertMatrix(gradGates[currentTimeStep], false), ConvertMatrix(W, true));
                gradOutNext[currentTimeStep] = MatrixMultiplication(ConvertMatrix(gradGates[currentTimeStep], false), ConvertMatrix(U, true));
            }

            float[][] gradW = MatrixMultiplication(ConvertMatrix(gradGates[0], false), backpropValues[0][0]);
            float[][] gradU = MatrixMultiplication(ConvertMatrix(gradGates[1], false), backpropValues[0][1]);
            float[][] gradB = ConvertMatrix(gradGates[0], false);

            for(int i = 1; i < timeStep; i++) {
                gradW = MatrixAddition(gradW, (MatrixMultiplication(ConvertMatrix(gradGates[i], false), backpropValues[i][0])));
                gradU = MatrixAddition(gradU, (MatrixMultiplication(ConvertMatrix(gradGates[i + 1], false), backpropValues[i][1])));
                gradB = MatrixAddition(gradB, ConvertMatrix(gradGates[i], false));
            }

            string output = "";
            for(int i = 0; i < W[0].Length; i++) {
                output += W[0][i] + " ";
            }

            Debug.Log("wc before: " + output);

            //ConvertMatrix(W, true) - learning rate * gradW 
            W = ReversMatrix(matrixSubtraction(ConvertMatrix(W, true), MatrxiScalerMultiplication(gradW, learningRate)), true);
            
            output = "";
            for (int i = 0; i < W[0].Length; i++) {
                output += W[0][i] + " ";
            }

            Debug.Log("wc before: " + output);

            wc = W[0];
            wi = W[1];
            wf = W[2];
            wo = W[3];

            U = ReversMatrix(matrixSubtraction(ConvertMatrix(U, true), MatrxiScalerMultiplication(gradU, learningRate)), true);
            uc = U[0];
            ui = U[1];
            uf = U[2];
            uo = U[3];
            
            B = ReversMatrix(matrixSubtraction(ConvertMatrix(B, true), MatrxiScalerMultiplication(gradB, learningRate)), true);
            bc = B[0];
            bi = B[1];
            bf = B[2];
            bo = B[3];

            ResetBacpropValues();
        }
    }

    private float[][] MatrxiScalerMultiplication(float[][] matrix, float value) {
        int row = matrix.Length;
        int cols = matrix[0].Length;
        float[][] result = CreateMatrix(row, cols);

        for(int i = 0; i < row; i++)
            for(int j = 0; j < cols; j++)
                result[i][j] = matrix[i][j] * value;

        return result;
    }

    private float[][] matrixSubtraction(float[][] matrixA, float[][] matrixB) {

        int row = matrixA.Length;
        int cols = matrixA[0].Length;
        float[][] result = CreateMatrix(row, cols);

        for(int i = 0; i < row; i++)
            for(int j = 0; j < cols; j++)
                result[i][j] = matrixA[i][j] - matrixB[i][j];

        return result;
    }

    private float[][] ConvertMatrix(float[][][] matrix, bool vertical) {
        float[][] result;

        if(!vertical) {
            result = new float[matrix.Length * matrix[0].Length][];
            
            for(int i = 0; i < matrix.Length; i++){

                for(int j = 0; j < matrix[0].Length; j++) {

                    float[] temp = new float[matrix[i][j].Length];
                    for(int n = 0; n < matrix[i][j].Length; n++) {
                        temp[n] = matrix[i][j][n];
                    }

                    result[i * matrix.Length + j] = temp;
                }
            }
        } else {
            result = new float[matrix[0].Length][];

            for(int i = 0; i < result.Length; i++) {
                result[i] = new float[matrix[0][0].Length * matrix.Length]; 
            }

            for(int j = 0; j < matrix[0].Length; j++) {
                for(int i = 0; i < matrix.Length; i++) {
                    for(int h = 0; h < matrix[i][j].Length; h++) {
                        result[j][i * matrix[i][j].Length + h] = matrix[i][j][h];
                    }
                }

            }
        }
        return result;
    }

    private float[][][] ReversMatrix(float[][] matrix, bool vertical) {
        float[][][] result = new float[4][][];

        if(!vertical) {
            int newWidth = matrix.Length / result.Length;

            for(int i = 0; i < result.Length; i++) {
                
                result[i] = new float[newWidth][];
                for(int width = 0; width < newWidth; width++) {
                    
                    result[i][width] = new float[matrix[0].Length];
                    for(int h = 0; h < matrix[width].Length; h++) {
                        
                        result[i][width][h] = matrix[i * newWidth + width][h];
                    }
                }
            }
        } else {

            int newDepth = matrix[0].Length / result.Length;

            for(int i = 0; i < result.Length; i++) {
                
                result[i] = new float[matrix.Length][];
                for(int length = 0; length < result[i].Length; length++) {

                    result[i][length] = new float[newDepth];
                    for(int depth = 0; depth < newDepth; depth++) {

                        result[i][length][depth] = matrix[length][i * newDepth + depth];
                    }
                }
            }
        }

        return result;
    }

    private float[][] MinusOneInvers(float[][] matrixA)
    {
    
        int row = matrixA.Length;
        int cols = matrixA[0].Length;
        float[][] result = CreateMatrix(row, cols);
    
        for (int i = 0; i < row; i++)
            for (int j = 0; j < cols; j++)
                result[i][j] = 1 - matrixA[i][j];
    
        return result;
    }
    
    private float[][] ZeroMatrix(float[][] matrixA)
    {
        int row = matrixA.Length;
        int cols = matrixA[0].Length;
        float[][] result = CreateMatrix(row, cols);
    
        for (int i = 0; i < row; i++)
            for (int j = 0; j < cols; j++)
                result[i][j] = 0;
    
        return result;
    }
    
    private float[][] ElementalWiseSubtraction(float[][] matrixA, float[][] matrixB)
    {
        if (matrixA.Length != matrixB.Length || matrixA[0].Length != matrixB[0].Length)
        {
            Debug.LogError("Matrices do not match");
            return null;
        }
    
        int row = matrixA.Length;
        int cols = matrixA[0].Length;
        float[][] result = CreateMatrix(row, cols);
    
        for (int i = 0; i < row; i++)
            for (int j = 0; j < cols; j++)
                result[i][j] = matrixA[i][j] - matrixB[i][j];
    
        return result;
    }
    
    private float[][] ElementWiseLogSigmoid(float[][] matrix)
    {
        int rows = matrix.Length;
        int cols = matrix[0].Length;
        float[][] result = CreateMatrix(rows, cols);
    
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                result[i][j] = 1 / (1 + Mathf.Exp(-matrix[i][j]));
    
        return result;
    }
    
    private float[][] ElementWiseTanh(float[][] matrix)
    {
        int rows = matrix.Length;
        int cols = matrix[0].Length;
        float[][] result = CreateMatrix(rows, cols);
    
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                result[i][j] = (float)System.Math.Tanh(matrix[i][j]);
    
        return result;
    }
    
    
    private float[][] ElementWiseTanh2(float[][] matrix)
    {
        int rows = matrix.Length;
        int cols = matrix[0].Length;
        float[][] result = CreateMatrix(rows, cols);
    
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                result[i][j] = 1 - ((float)System.Math.Tanh(matrix[i][j]) * (float)System.Math.Tanh(matrix[i][j]));
    
        return result;
    }
    
    
    private float[][] CreateMatrix(int rows, int cols)
    {
        float[][] result = new float[rows][];
    
        for (int i = 0; i < rows; i++)
            result[i] = new float[cols];
    
        return result;
    }
    
    private float[][] ConvertArrayToMatrix(float[] array, int rows, int cols)
    {
        float[][] result = CreateMatrix(rows, cols);
        int pointer = 0;
    
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                result[i][j] = array[pointer++];
    
        return result;
    }
    
    private float[][] CopyMatrix(float[][] matrix)
    {
        int rows = matrix.Length;
        int cols = matrix[0].Length;
        float[][] result = CreateMatrix(rows, cols);
    
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                result[i][j] = matrix[i][j];
    
        return result;
    }
    
    private float[][] ElementWiseMultiplication(float[][] matrixA, float[][] matrixB)
    {
    
        if (matrixA.Length != matrixB.Length || matrixA[0].Length != matrixB[0].Length)
        {
            Debug.LogError("Matrix not of same size");
            return null;
        }
    
        int rows = matrixA.Length;
        int cols = matrixA[0].Length;
        float[][] result = CreateMatrix(rows, cols);
    
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                result[i][j] = matrixA[i][j] * matrixB[i][j];
    
        return result;
    }
    
    private float[][] MatrixMultiplication(float[][] matrixB, float[][] matrixA)
    {
        if (matrixA.Length != matrixB[0].Length)
        {
            Debug.LogError("Matxrix A and B cant be multipled cols in A =/= rows in B");
            return null;
        }
    
        int rows = matrixB.Length;
        int cols = matrixA[0].Length;
        float[][] result = CreateMatrix(rows, cols);
    
        int rowsA = matrixA.Length;
    
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
    
                float[] aLen = new float[rowsA];
                for (int n = 0; n < aLen.Length; n++)
                {
                    aLen[n] = matrixA[n][j];
                }
    
                result[i][j] = MatrixSum(aLen, matrixB[i]);
            }
        }
    
        return result;
    }
    
    private float MatrixSum(float[] arrayA, float[] ArrayB)
    {
        float result = 0;
    
        for (int i = 0; i < arrayA.Length; i++)
            result += arrayA[i] * ArrayB[i];
    
        return result;
    }
    
    private float[][] MatrixAddition(float[][] matrixA, float[][] matrixB)
    {
    
        if (matrixA.Length != matrixB.Length || matrixA[0].Length != matrixB[0].Length)
        {
            Debug.LogError("Matrix not of same size");
            return null;
        }
    
        int row = matrixA.Length;
        int cols = matrixA[0].Length;
        float[][] result = CreateMatrix(row, cols);
    
        for (int i = 0; i < row; i++)
            for (int j = 0; j < cols; j++)
                result[i][j] = matrixA[i][j] + matrixB[i][j];
    
        return result;
    
    }
    
    private float[][] MatrixAddition(float[][] matrixA, float[][] matrixB, float[][] matrixC)
    {
    
        if (matrixA.Length != matrixB.Length || matrixA[0].Length != matrixB[0].Length || matrixB.Length != matrixC.Length || matrixB[0].Length != matrixC[0].Length)
        {
            Debug.LogError("Matrix not of same size");
            return null;
        }
    
        int row = matrixA.Length;
        int cols = matrixA[0].Length;
        float[][] result = CreateMatrix(row, cols);
    
        for (int i = 0; i < row; i++)
            for (int j = 0; j < cols; j++)
                result[i][j] = matrixA[i][j] + matrixB[i][j] + matrixC[i][j];
    
        return result;
    
    }
    
    private void DebugTestMatrixMultiplication( /*float[][] a, float[][] b*/)
    {
        float[][] a = { new float[] { 1 }, new float[] { 4 }, new float[] { 6 } };
        float[][] b = { new float[] { 2, 3, 4 }, new float[] { 1, 5, 3 }, new float[] { 2, 6, 2 }, new float[] { 3, 7, 1 } };
    
        float[][] test = MatrixMultiplication(b, a);
        List<List<string>> strs = new List<List<string>>();
        for (int i = 0; i < test.Length; i++)
        {
            List<string> s = new List<string>();
            for (int j = 0; j < test[i].Length; j++)
            {
                s.Add(test[i][j] + " ");
            }
            strs.Add(s);
        }
    
        string ut = "";
    
        for (int i = 0; i < strs[0].Count; i++)
        {
            for (int j = 0; j < strs.Count; j++)
            {
                ut += strs[j][i] + " ";
            }
            ut += " \n";
        }
        Debug.Log(ut);
    }

}

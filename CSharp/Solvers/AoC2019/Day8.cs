﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AdventOfCode.Solvers.Base;
using AdventOfCode.Utils;

namespace AdventOfCode.Solvers.AoC2019
{
    /// <summary>
    /// Solver for 2019 Day 8
    /// </summary>
    public class Day8 : Solver<Day8.Layer[]>
    {
        /// <summary>
        /// Image layer
        /// </summary>
        public class Layer : IEnumerable<int>
        {
            #region Constants
            /// <summary>
            /// Layer width
            /// </summary>
            public const int WIDTH = 25;
            /// <summary>
            /// Layer height
            /// </summary>
            public const int HEIGHT = 6;
            /// <summary>
            /// Total layer size
            /// </summary>
            public const int SIZE = WIDTH * HEIGHT;
            #endregion

            #region Fields
            public readonly int[,] image = new int[HEIGHT, WIDTH];
            #endregion
            
            #region Indexers
            /// <summary>
            /// Accesses the underlying image of the Layer
            /// </summary>
            /// <param name="i">Horizontal address</param>
            /// <param name="j">Vertical address</param>
            /// <returns>The value at the given address</returns>
            public int this[int i, int j]
            {
                get => this.image[j, i];
                set => this.image[j, i] = value;
            }
            #endregion

            #region Constructors
            /// <summary>
            /// Creates a new layer from given data
            /// </summary>
            /// <param name="data">Data to create the layer from</param>
            /// <exception cref="ArgumentOutOfRangeException">If the data is not of the correct size</exception>
            public Layer(string data)
            {
                //Make sure the data passed if of the right length
                if (data.Length is not SIZE) throw new ArgumentOutOfRangeException(nameof(data), data.Length, $"Data length must be {SIZE}");
                
                //Parse data
                foreach (int j in ..HEIGHT)
                {
                    int stride = j * WIDTH;
                    foreach (int i in ..WIDTH)
                    {
                        this[i, j] = data[stride + i] - '0';
                    }
                }
            }

            /// <summary>
            /// Creates a new blank layer
            /// </summary>
            public Layer() { }
            #endregion

            #region Methods
            /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
            public IEnumerator<int> GetEnumerator() => this.image.Cast<int>().GetEnumerator();
            
            /// <inheritdoc cref="IEnumerable.GetEnumerator"/>
            IEnumerator IEnumerable.GetEnumerator() => this.image.GetEnumerator();

            /// <inheritdoc cref="object.ToString"/>
            public override string ToString()
            {
                StringBuilder sb = new(SIZE + HEIGHT);
                foreach (int j in ..HEIGHT)
                {
                    foreach (int i in ..WIDTH)
                    {
                        char color = this[i, j] switch
                        {
                            0 => '░',
                            1 => '▓',
                            _ => ' '
                        };
                        sb.Append(color);
                    }
                    sb.AppendLine();
                }

                return sb.ToString();
            }
            #endregion
        }
        
        #region Constructors
        /// <summary>
        /// Creates a new <see cref="Day8"/> Solver with the input data properly parsed
        /// </summary>
        /// <param name="file">Input file</param>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="file"/> does not exist or has an invalid extension</exception>
        /// <exception cref="FileLoadException">Thrown if the input <paramref name="file"/> could not be properly loaded</exception>
        /// <exception cref="InvalidOperationException">Thrown if the conversion to <see cref="T"/> fails</exception>
        public Day8(FileInfo file) : base(file) { }
        #endregion

        #region Methods
        /// <inheritdoc cref="Solver.Run"/>
        public override void Run()
        {
            Layer smallest = this.Data[0];
            int zeroes = smallest.Count(i => i is 0);
            foreach (Layer layer in this.Data[1..])
            {
                int z = layer.Count(i => i is 0);
                if (z < zeroes)
                {
                    zeroes = z;
                    smallest = layer;
                }
            }

            int ones = 0, twos = 0;
            foreach (int i in smallest)
            {
                switch (i)
                {
                    case 1:
                        ones++;
                        break;
                    
                    case 2:
                        twos++;
                        break;
                }
            }
            
            AoCUtils.LogPart1(ones * twos);

            Layer image = new();
            foreach (int i in ..Layer.WIDTH)
            {
                foreach (int j in ..Layer.HEIGHT)
                {
                    foreach (Layer layer in this.Data)
                    {
                        if (layer[i, j] is not 2)
                        {
                            image[i, j] = layer[i, j];
                            break;
                        }
                    }
                }
            }
            
            AoCUtils.LogPart2(string.Empty);
            Trace.WriteLine(image);
        }

        /// <inheritdoc cref="Solver{T}.Convert"/>
        public override Layer[] Convert(string[] rawInput)
        {
            string line = rawInput[0];
            List<Layer> layers = new();
            for (int i = 0, j = Layer.SIZE; j <= line.Length; i = j, j += Layer.SIZE)
            {
                layers.Add(new Layer(line[i..j]));
            }

            return layers.ToArray();
        }
        #endregion
    }
}

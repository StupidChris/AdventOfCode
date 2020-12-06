﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AdventOfCode.Solvers.Base;
using AdventOfCode.Utils;
using AdventOfCode.Utils.Vectors;

namespace AdventOfCode.Solvers
{
    public class Day3: Solver<Grid<bool>>
    {
        #region Constructors
        /// <summary>
        /// Creates a new generic <see cref="Solver{T}"/> with the input data properly parsed
        /// </summary>
        /// <param name="file">Input file</param>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="file"/> does not exist or has an invalid extension</exception>
        /// <exception cref="FileLoadException">Thrown if the input <paramref name="file"/> could not be properly loaded</exception>
        /// <exception cref="InvalidOperationException">Thrown if the conversion to <see cref="Grid{T}"/> fails</exception>
        public Day3(FileInfo file) : base(file) { }
        #endregion

        #region Methods
        /// <inheritdoc cref="Solver"/>
        public override void Run()
        {
            //Part one
            long result = CheckSlope(new Vector2(3, 1));
            Trace.WriteLine(result);
            
            //Part two
            result *= CheckSlope(new Vector2(1, 1));
            result *= CheckSlope(new Vector2(5, 1));
            result *= CheckSlope(new Vector2(7, 1));
            result *= CheckSlope(new Vector2(1, 2));
            Trace.WriteLine(result);
        }

        /// <summary>
        /// Check for collisions on a given slope
        /// </summary>
        /// <param name="slope">Slope to check</param>
        /// <returns>Amount of tree hit on this slope</returns>
        private int CheckSlope(in Vector2 slope)
        {
            int hits = 0;
            Vector2? position = slope;
            do
            {
                //Check the position for a hit
                if (this.Input[position.Value])
                {
                    hits++;
                }
                //Move along slope
                position = this.Input.MoveWithinGrid(position.Value, slope, true);
            }
            while (position is not null); //Keep moving until out of bounds at the bottom

            return hits;
        }

        /// <inheritdoc cref="Solver{T}"/>
        public override Grid<bool> Convert(string[] rawInput)
        {
            int width = rawInput[0].Length;
            int height = rawInput.Length;
            return new Grid<bool>(width, height, rawInput, s => s.Select(c => c is '#').ToArray());
        }
        #endregion
    }
}
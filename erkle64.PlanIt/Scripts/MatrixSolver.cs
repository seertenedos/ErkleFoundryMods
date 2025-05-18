using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlanIt
{
    internal class MatrixSolver
    {
        private ItemElementTemplate[] _items;
        private Dictionary<ItemElementTemplate, int> _itemIndices;
        private HashSet<ItemElementTemplate> _outputs;
        private ItemElementTemplate[] _outputItems;
        private ItemElementRecipe[] _inputRecipes;
        private ItemElementRecipe[] _recipes;
        private Dictionary<ulong, int> _recipeIndices;
        private double[,] _recipeMatrix;

        public ItemElementRecipe[] InputRecipes => _inputRecipes;
        public HashSet<ItemElementTemplate> Outputs => _outputs;

        public MatrixSolver(ItemElementRecipe[] recipes)
        {
            var products = new HashSet<ItemElementTemplate>();
            var ingredients = new HashSet<ItemElementTemplate>();
            var recipeList = new List<ItemElementRecipe>();
            foreach (var recipe in recipes)
            {
                recipeList.Add(recipe);
                foreach (var item in recipe.outputs)
                {
                    products.Add(item.itemElement);
                }
                foreach (var item in recipe.inputs)
                {
                    ingredients.Add(item.itemElement);
                }
            }

            _outputs = new HashSet<ItemElementTemplate>();
            _outputItems = new ItemElementTemplate[products.Count];

            var items = new List<ItemElementTemplate>();
            var wasteItems = new Dictionary<ItemElementTemplate, int>();
            foreach (var item in products)
            {
                _outputs.Add(item);
                wasteItems[item] = items.Count;
                _outputItems[items.Count] = item;
                items.Add(item);
            }

            var inputRecipes = new List<ItemElementRecipe>();
            foreach (var item in ingredients)
            {
                if (products.Contains(item)) continue;
                items.Add(item);
                var itemRecipes = item.GetRecipes();
                inputRecipes.AddRange(itemRecipes);
                //inputRecipes.Add(itemRecipes[0]);
                //if (itemRecipes.Length > 0) inputRecipes.Add(itemRecipes[0]);
                //if (itemRecipes.Length > 0) inputRecipes.AddRange(itemRecipes);
                //else inputRecipes.Add(null);
            }
            _items = items.ToArray();
            _inputRecipes = inputRecipes.ToArray();

            var combinedRecipes = new List<ItemElementRecipe>(recipeList);
            combinedRecipes.AddRange(_inputRecipes);
            _recipes = combinedRecipes.ToArray();
            _itemIndices = new Dictionary<ItemElementTemplate, int>();
            for (int i = 0; i < _items.Length; i++)
            {
                _itemIndices[_items[i]] = i;
            }

            _recipeIndices = new Dictionary<ulong, int>();
            for (var i = 0; i < _recipes.Length; i++)
            {
                _recipeIndices[_recipes[i].id] = i;
            }

            var rows = _recipes.Length + 2;
            var columns = _items.Length + _recipes.Length + 3;
            _recipeMatrix = new double[rows, columns];
            _recipeMatrix.Fill(0.0);
            for (var i = 0; i < recipeList.Count; i++)
            {
                var recipe = recipeList[i];
                var recipeIngredients = recipe.inputs;
                foreach (var ingredient in recipeIngredients)
                {
                    _recipeMatrix[i, _itemIndices[ingredient.itemElement]] -= ingredient.amount;
                }

                var recipeProducts = recipe.outputs;
                foreach (var product in recipeProducts)
                {
                    _recipeMatrix[i, _itemIndices[product.itemElement]] += product.amount;
                }

                _recipeMatrix[i, _items.Length] = -1.0;
            }

            for (var i = 0; i < inputRecipes.Count; i++)
            {
                var recipe = inputRecipes[i];
                foreach (var product in recipe.outputs)
                {
                    if (_itemIndices.TryGetValue(product.itemElement, out var k))
                    {
                        _recipeMatrix[i + recipeList.Count, k] += product.amount;
                    }
                }
            }

            _recipeMatrix[_recipes.Length, _items.Length] = 1.0;
            for (var i = 0; i < _recipes.Length; i++)
            {
                _recipeMatrix[i, _items.Length + i + 1] = 1.0;
            }
            _recipeMatrix[rows - 1, _items.Length + _recipes.Length + 1] = 1.0;
        }

        public Dictionary<ItemElementTemplate, double> Match(Dictionary<ItemElementTemplate, double> products)
        {
            var result = new Dictionary<ItemElementTemplate, double>();
            foreach (var product in products)
            {
                if (product.Value > 0.0 && _outputs.Contains(product.Key))
                {
                    result[product.Key] = product.Value;
                }
            }
            return result;
        }

        public double GetPriorityRatio(double[,] matrix)
        {
            var min = double.MaxValue;
            var max = double.MinValue;
            foreach (var value in matrix)
            {
                var x = Math.Abs(value);
                if (x == 0.0) continue;

                if (x < min) min = x;
                if (x > max) max = x;
            }
            return max / min;
        }

        public void SetCost(double[,] matrix)
        {
            matrix[_recipes.Length, matrix.GetLength(1) - 1] = 1.0;
            //var ratio = GetPriorityRatio(matrix);
            //var cost = ratio;
            for (int i = _recipes.Length - 1; i >= 0; i--)
            {
                var totalOutput = 0.0;
                var totalInput = 0.0;
                for (int j = 0; j < _items.Length; j++)
                {
                    if (matrix[i, j] > 0.0)
                    {
                        totalOutput += matrix[i, j];
                    }
                    else if (matrix[i, j] < 0.0)
                    {
                        if (_items[j].identifier == "_base_biomass")
                            totalInput -= matrix[i, j] * 100.0;
                        else
                            totalInput -= matrix[i, j];
                    }
                }

                //var total = 1.0;
                //for (int j = 0; j < _items.Length; j++)
                //    if (matrix[i, j] < 0.0)
                //        total -= matrix[i, j];

                matrix[i, matrix.GetLength(1) - 1] = totalOutput * totalInput;
                //cost *= ratio;
            }
        }

        public (Dictionary<ItemElementRecipe, double>, Dictionary<ItemElementTemplate, double>) Solve(Dictionary<ItemElementTemplate, double> products, IEnumerable<ulong> disabledRecipes)
        {
            var matrix = new double[_recipeMatrix.GetLength(0), _recipeMatrix.GetLength(1)];
            Array.Copy(_recipeMatrix, matrix, _recipeMatrix.Length);
            foreach (var product in products)
            {
                if (_itemIndices.TryGetValue(product.Key, out var column))
                {
                    //PlanItSystem.log.Log($"matrix[{matrix.GetLength(0) - 1}, {column}] = -{product.Value}");
                    matrix[matrix.GetLength(0) - 1, column] = -product.Value;
                }
            }

            foreach (var recipeId in disabledRecipes)
            {
                if (_recipeIndices.TryGetValue(recipeId, out var row))
                {
                    //PlanItSystem.log.Log($"Disabling recipe: {ItemElementRecipe.Get(recipeId)?.name ?? "null"}");
                    var columnCount = matrix.GetLength(1);
                    for (int i = 0; i < columnCount; i++)
                    {
                        matrix[row, i] = 0.0;
                    }
                }
            }

            SetCost(matrix);

            Simplex(matrix);

            var solution = new Dictionary<ItemElementRecipe, double>();
            for (var i = 0; i < _recipes.Length; i++)
            {
                var column = _items.Length + i + 1;
                var rate = matrix[matrix.GetLength(0) - 1, column];
                if (rate > 0.0)
                {
                    solution[_recipes[i]] = rate;
                }
            }

            var waste = new Dictionary<ItemElementTemplate, double>();
            for (var i = 0; i < _outputItems.Length; i++)
            {
                var rate = matrix[matrix.GetLength(0) - 1, i];
                if (rate > 0.0)
                {
                    waste[_outputItems[i]] = rate;
                }
            }

            return (solution, waste);
        }

        private void Simplex(double[,] matrix)
        {
            var limit = 500;
            while (limit-- > 0)
            {
                //PlanItSystem.log.Log($"Simplex:\n{MatrixToString(matrix)}");
                var min = double.MaxValue;
                var minColumn = -1;
                for (var column = 0; column < matrix.GetLength(1) - 1; column++)
                {
                    var x = matrix[matrix.GetLength(0) - 1, column];
                    if (x < min)
                    {
                        min = x;
                        minColumn = column;
                    }
                }
                if (min >= 0.0) return;

                if (!PivotColumn(matrix, minColumn)) return;
            }

            PlanItSystem.log.LogWarning($"Reached limit!\n{MatrixToString(matrix)}");
        }

        private bool PivotColumn(double[,] matrix, int column)
        {
            var minRatio = double.MaxValue;
            var minRow = -1;
            for (var row = 0; row < matrix.GetLength(0) - 1; row++)
            {
                var x = matrix[row, column];
                if (x <= 0.0) continue;

                var ratio = matrix[row, matrix.GetLength(1) - 1] / x;
                if (ratio < minRatio)
                {
                    minRatio = ratio;
                    minRow = row;
                }
            }

            if (minRow >= 0)
            {
                Pivot(matrix, minRow, column);
                return true;
            }

            return false;
        }

        private void Pivot(double[,] matrix, int row, int column)
        {
            var x = matrix[row, column];
            for (int i = 0; i < matrix.GetLength(1); i++) matrix[row, i] /= x;

            for (var r = 0; r < matrix.GetLength(0); r++)
            {
                if (r == row) continue;

                var ratio = matrix[r, column];
                if (ratio >= -double.Epsilon && ratio <= double.Epsilon) continue;

                for (var c = 0; c < matrix.GetLength(1); c++)
                {
                    matrix[r, c] -= matrix[row, c] * ratio;
                }
            }
        }

        private void AppendCell(StringBuilder sb, string text, int width = 24)
        {
            var textWidth = Mathf.Min(width - 1, text.Length);
            sb.Append(text[..textWidth]);
            for (var i = textWidth; i < width; i++) sb.Append(" ");
        }

        private string MatrixToString(double[,] matrix)
        {
            var sb = new StringBuilder();
            AppendCell(sb, string.Empty);
            foreach (var item in _items)
            {
                AppendCell(sb, item.name);
            }
            AppendCell(sb, "tax");
            foreach (var recipe in _recipes)
            {
                AppendCell(sb, recipe.name);
            }
            AppendCell(sb, "cost");
            sb.AppendLine();
            for (var row = 0; row < matrix.GetLength(0); row++)
            {
                if (row < _recipes.Length)
                {
                    AppendCell(sb, _recipes[row].name);
                }
                else if (row == _recipes.Length)
                {
                    AppendCell(sb, "tax");
                }
                else
                {
                    AppendCell(sb, "target");
                }
                for (var column = 0; column < matrix.GetLength(1); column++)
                {
                    AppendCell(sb, $"{(float)matrix[row, column]:0.##}");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}

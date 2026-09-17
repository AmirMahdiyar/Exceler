using Exceler.Configuration;
using System;
using System.Runtime.CompilerServices;

namespace Exceler.Core.OpenXml
{
    /// <summary>
    /// Evaluates registered column and row conditional style rules during OpenXML SAX row streaming
    /// and resolves the appropriate pre-compiled style index in $O(1)$ time without memory allocations.
    /// </summary>
    internal class ColumnStyleResolver
    {
        /// <summary>
        /// Gets or sets the 1-based column index.
        /// </summary>
        public int ColumnIndex { get; set; }

        /// <summary>
        /// Gets or sets the default base style index for this column.
        /// </summary>
        public uint BaseStyleIndex { get; set; }

        /// <summary>
        /// Gets or sets the array of compiled column-level conditional rules and their corresponding style indices.
        /// Column-level rules take the highest precedence.
        /// </summary>
        public (IConditionalStyleRule Rule, uint StyleIndex)[] ColumnRules { get; set; } = Array.Empty<(IConditionalStyleRule, uint)>();

        /// <summary>
        /// Gets or sets the array of compiled row-level conditional rules and their corresponding style indices.
        /// Evaluated when no column-level rule matches.
        /// </summary>
        public (IConditionalStyleRule Rule, uint StyleIndex)[] RowRules { get; set; } = Array.Empty<(IConditionalStyleRule, uint)>();

        /// <summary>
        /// Evaluates registered conditional rules against the row entity and returns the target style index.
        /// </summary>
        /// <param name="item">The row data model entity, if any.</param>
        /// <returns>The resolved style index to emit in cell attributes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ResolveStyle(object? item)
        {
            if (item == null) return BaseStyleIndex;
            // 1. Check column-level conditional rules first (highest precedence)
            for (int i = 0; i < ColumnRules.Length; i++)
            {
                if (ColumnRules[i].Rule.Evaluate(item))
                {
                    return ColumnRules[i].StyleIndex;
                }
            }

            // 2. Check row-level conditional rules second
            for (int i = 0; i < RowRules.Length; i++)
            {
                if (RowRules[i].Rule.Evaluate(item))
                {
                    return RowRules[i].StyleIndex;
                }
            }

            // 3. Fallback to base column style
            return BaseStyleIndex;
        }
    }
}

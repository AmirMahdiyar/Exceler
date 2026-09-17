using System;

namespace Exceler.Configuration
{
    /// <summary>
    /// Represents a conditional styling rule applied during Excel export streaming.
    /// </summary>
    public interface IConditionalStyleRule
    {
        /// <summary>
        /// Evaluates whether the condition is met for the specified row entity.
        /// </summary>
        /// <param name="item">The row data model instance.</param>
        /// <returns><c>true</c> if the condition matches; otherwise, <c>false</c>.</returns>
        bool Evaluate(object item);

        /// <summary>
        /// Gets the visual styling overrides applied when this rule matches.
        /// </summary>
        ColumnStyle Style { get; }
    }

    /// <summary>
    /// Strongly-typed implementation of <see cref="IConditionalStyleRule"/> evaluating conditions against <typeparamref name="TModel"/>.
    /// </summary>
    /// <typeparam name="TModel">The row data model type.</typeparam>
    public class ConditionalStyleRule<TModel> : IConditionalStyleRule where TModel : class
    {
        private readonly Func<TModel, bool> _condition;

        /// <summary>
        /// Gets the visual styling overrides applied when this rule matches.
        /// </summary>
        public ColumnStyle Style { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalStyleRule{TModel}"/> class.
        /// </summary>
        /// <param name="condition">The strongly-typed predicate evaluating whether the rule matches.</param>
        /// <param name="style">The styling overrides applied upon match.</param>
        public ConditionalStyleRule(Func<TModel, bool> condition, ColumnStyle style)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Style = style ?? throw new ArgumentNullException(nameof(style));
        }

        /// <inheritdoc />
        public bool Evaluate(object item)
        {
            if (item is TModel model)
            {
                return _condition(model);
            }
            return false;
        }
    }
}

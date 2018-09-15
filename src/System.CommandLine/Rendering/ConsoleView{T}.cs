using System.Collections.Generic;

namespace System.CommandLine.Rendering
{
    public abstract class ConsoleView<T> : IConsoleView<T>
    {
        private Region _effectiveRegion;
        private int _verticalOffset;

        protected ConsoleView(
            ConsoleRenderer renderer,
            Region region = null)
        {
            ConsoleRenderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            Region = region ?? renderer.Console.GetRegion();

            SetEffectiveRegion();
        }

        protected ConsoleRenderer ConsoleRenderer { get; }

        public Region Region { get; }

        public virtual void Render(T value)
        {
            SetEffectiveRegion();

            _verticalOffset = 0;

            OnRender(value);
        }

        protected abstract void OnRender(T value);

        public void RenderTable<TItem>(
            IReadOnlyCollection<TItem> items,
            Action<ConsoleTable<TItem>> renderColumnns)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (renderColumnns == null)
            {
                throw new ArgumentNullException(nameof(renderColumnns));
            }

            var tableView = new ConsoleTable<TItem>(ConsoleRenderer);

            renderColumnns(tableView);

            int columnLeftPosition = 0;

            foreach (var column in tableView.Columns)
            {
                column.Left = columnLeftPosition;
                column.BuildCells(items);
                columnLeftPosition += column.Width;
            }

            int columnCount = tableView.Columns.Count;

            for (int rowIdx = 0; rowIdx <= items.Count; rowIdx++)
            {
                int topPosition = _verticalOffset + rowIdx;
                int colIdx = 0;

                foreach (var column in tableView.Columns)
                {
                    colIdx++;
                    column.FlushCell(
                        rowIdx,
                        topPosition,
                        colIdx == columnCount,
                        ConsoleRenderer);
                }
            }
        }

        public void WriteLine()
        {
            if (_effectiveRegion.Height <= 1)
            {
                return;
            }

            _verticalOffset++;

            _effectiveRegion = new Region(
                _effectiveRegion.Left,
                _effectiveRegion.Top + 1,
                _effectiveRegion.Width,
                _effectiveRegion.Height - 1,
                false);
        }

        public void Write(object value)
        {
            ConsoleRenderer.RenderToRegion(value, _effectiveRegion);
        }

        public void WriteLine(object value)
        {
            Write(value);
            WriteLine();
        }

        protected Span Span(FormattableString formattable) =>
            ConsoleRenderer.Formatter.ParseToSpan(formattable);

        protected Span Span(object value) =>
            ConsoleRenderer.Formatter.Format(value);

        private void SetEffectiveRegion()
        {
            _effectiveRegion = new Region(
                Region.Left,
                Region.Top,
                Region.Width,
                Region.Height,
                false);
        }
    }
}

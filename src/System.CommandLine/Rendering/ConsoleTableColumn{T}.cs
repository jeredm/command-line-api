using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Rendering
{
    internal class ConsoleTableColumn<T>
    {
        private List<Span> _cells;

        public ConsoleTableColumn(
            Span header,
            Func<T, Span> renderCell)
        {
            RenderCell = renderCell ?? throw new ArgumentNullException(nameof(renderCell));
            Header = header;
        }

        public Func<T, Span> RenderCell { get; }

        public Span Header { get; }

        public int Gutter { get; set; } = 2;

        public int Left { get; internal set; }

        public int Height { get; set; } = 1;

        public int Width { get; private set; }

        public void BuildCells(IReadOnlyCollection<T> cellItems)
        {
            _cells = cellItems
                .Select(RenderCell)
                .ToList();

            _cells.Insert(0, Header);

            UpdateWidth();
        }

        public void FlushCell(
            int rowIdx,
            int top,
            bool isLastColumn,
            ConsoleRenderer consoleRenderer)
        {
            var region = new Region(
                left: Left,
                top: top,
                width: Width,
                height: Height);

            var cell = _cells[rowIdx];

            if (isLastColumn)
            {
                cell = new ContainerSpan(cell, new ContentSpan(Environment.NewLine));
            }

            consoleRenderer.RenderToRegion(cell, region);
        }

        private void UpdateWidth()
        {
            Width = Gutter + _cells.Max(s => s.ContentLength);
        }
    }
}

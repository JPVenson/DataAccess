using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTestProject1
{
    public interface IConsolePropertyGridStyle
    {
        int RenderHeader(ColloredStringBuilder stream, List<string> columnHeader);

        void BeginRenderProperty(ColloredStringBuilder stream, int elementNr, int maxElements, bool isSelected, bool focused);
        void RenderNextProperty(ColloredStringBuilder stream, string propertyValue, int elementNr, bool isSelected, bool focused);
        void EndRenderProperty(ColloredStringBuilder stream, int elementNr, bool isSelected, bool focused);

        void RenderFooter(ColloredStringBuilder stream, List<string> columnHeader);
        void RenderSummary(ColloredStringBuilder stream, int sum);
        void RenderAdditionalInfos(ColloredStringBuilder stream, StringBuilder additional);

        char VerticalLineSeperator { get; }
        char HorizontalLineSeperator { get; }

        char UpperLeftBound { get; }
        char LowerLeftBound { get; }

        char UpperRightBound { get; }
        char LowerRightBound { get; }

        ConsoleColor AlternatingTextStyle { get; set; }
        ConsoleColor AlternatingTextBackgroundStyle { get; set; }

        ConsoleColor SelectedItemBackgroundStyle { get; set; }
        ConsoleColor SelectedItemForgroundStyle { get; set; }

        ConsoleColor FocusedItemBackgroundStyle { get; set; }
        ConsoleColor FocusedItemForgroundStyle { get; set; }
    }
}
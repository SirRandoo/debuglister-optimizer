// MIT License
// 
// Copyright (c) 2021 SirRandoo
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace SirRandoo.DebugListerOptimizer
{
    public class DebugListListerDialog : Dialog_DebugOptionListLister
    {
        private bool _focusFilter;
        public DebugListListerDialog([NotNull] IEnumerable<DebugMenuOption> options) : base(options) { }

        public override void PostOpen()
        {
            base.PostOpen();
            if (!focusOnFilterOnOpen)
            {
                return;
            }

            _focusFilter = true;
            scrollPosition = Vector2.zero;
        }

        public override void DoWindowContents(Rect inRect)
        {
            GUI.SetNextControlName("DebugFilter");
            filter = Widgets.TextField(new Rect(0.0f, 0.0f, 200f, 30f), filter);
            if ((Event.current.type == EventType.KeyDown || Event.current.type == EventType.Repaint) && _focusFilter)
            {
                GUI.FocusControl("DebugFilter");
                filter = "";
                _focusFilter = false;
            }

            if (Event.current.type == EventType.Layout)
            {
                totalOptionsHeight = 0.0f;
            }

            var outRect = new Rect(inRect);
            outRect.yMin += 35f;
            var num = (int) (InitialSize.x / 200.0);
            float height = (totalOptionsHeight + 24f * (num - 1)) / num;
            if (height < (double) outRect.height)
            {
                height = outRect.height;
            }

            var rect = new Rect(0.0f, 0.0f, outRect.width - 16f, height);
            Widgets.BeginScrollView(outRect, ref scrollPosition, rect);
            listing = new Listing_Standard {ColumnWidth = (rect.width - 17f * (num - 1)) / num};
            listing.Begin(rect);
            DoListingItems(outRect.AtZero());
            listing.End();
            Widgets.EndScrollView();
        }

        private void DoListingItems(Rect viewRect)
        {
            int highlightedIndex = HighlightedIndex;
            for (var index = 0; index < options.Count; ++index)
            {
                DebugMenuOption option = options[index];
                bool highlight = highlightedIndex == index;

                if (!FilterAllows(option.label))
                {
                    continue;
                }

                Rect rect = listing.GetRect(22f);

                if (!rect.IsRegionVisible(viewRect, scrollPosition))
                {
                    TryIncrementHeight();
                    continue;
                }

                switch (option.mode)
                {
                    case DebugMenuOptionMode.Action:
                        DebugAction_New(rect, option.label, option.method, highlight);
                        break;
                    case DebugMenuOptionMode.Tool:
                        DebugToolMap_New(rect, option.label, option.method, highlight);
                        break;
                }
            }
        }

        private void TryIncrementHeight()
        {
            if (Event.current.type == EventType.Layout)
            {
                totalOptionsHeight += 24f;
            }
        }

        private void DebugAction_New(Rect region, string label, Action action, bool highlight)
        {
            if (ButtonDebug_New(region, label, highlight))
            {
                Close();
                action();
            }

            GUI.color = Color.white;
            TryIncrementHeight();
        }

        private void DebugToolMap_New(Rect region, string label, Action toolAction, bool highlight)
        {
            if (WorldRendererUtility.WorldRenderedNow)
            {
                return;
            }

            if (ButtonDebug_New(region, label, highlight))
            {
                Close();
                DebugTools.curTool = new DebugTool(label, toolAction);
            }

            GUI.color = Color.white;
            TryIncrementHeight();
        }

        private static bool ButtonDebug_New(Rect region, string label, bool highlight)
        {
            Text.Font = GameFont.Tiny;
            int num = Text.WordWrap ? 1 : 0;
            Text.WordWrap = false;
            bool flag = Widgets.ButtonText(region, label);
            Text.WordWrap = num != 0;

            if (!highlight)
            {
                return flag;
            }

            GUI.color = Color.yellow;
            Widgets.DrawBox(region, 2);
            GUI.color = Color.white;

            return flag;
        }
    }
}

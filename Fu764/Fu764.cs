#region usings
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security;

using Plugins;
using Plugins.Indicator;
using Plugins.Strategy;

using Tengoku.Kumo.Calc.Indicators;
using Tengoku.Kumo.Charting.Chart;
using Tengoku.Kumo.Charting.Frame;
using Tengoku.Kumo.Charting.Scale;

using Tengoku.VisualChart.Plugins.Attributes;
using Tengoku.VisualChart.Plugins.Types;
using VisualChart.Development.Runtime.DataSeries;
using VisualChart.Development.Runtime.Plugins;
#endregion

namespace Fu764
{
    /// <summary>
    /// Fibonacci based indicator (volume dependant)
    /// </summary>
    [Indicator(Name = "Fibonacci", Description = "Fibonacci based indicator (volume dependant)")]
    [Properties(Window = TargetWindow.DataSeriesWindow)]
    [OutputSeriesProperties(Line = 1, Name = "Fu1")]
    [OutputSeriesProperties(Line = 2, Name = "fu764")]
    [OutputSeriesProperties(Line = 3, Name = "basis")]
    [OutputSeriesProperties(Line = 4, Name = "Fd764")]
    [OutputSeriesProperties(Line = 5, Name = "Fd1")]
    public class Fu764 : IndicatorPlugin
    {
        [Parameter(Name = "Fibo Length", DefaultValue = 200, MinValue = 1, Step = 1)]
        private long fiboLength;

        [Parameter(Name = "Fibo Level", DefaultValue = 764, MinValue = 1, MaxValue = 1000, Step = 1)]
        private long fiboLevel;

        [Parameter(Name = "Fibo Multiplier", DefaultValue = 3, MinValue = 0.001, MaxValue = 50)]
        private double fiboMultiplier;

        MMPV basis;
        StandardDev standardDev;

        /// <summary>
        /// This method is used to configure the indicator and is called once before any indicator method is called.
        /// </summary>
        public override void OnInitCalculate()
        {
            //Volume weighted moving average
            basis = new MMPV(this.Data, fiboLength);
            standardDev = new StandardDev(this.Data, fiboLength);
        }

        /// <summary>
        /// Called on each bar update event.
        /// </summary>
        /// <param name="bar">Bar index.</param>
        public override void OnCalculateBar(int bar)
        {
            double currentDeviation = standardDev.Value(0, 1);
            double currentBasis = basis.Value(0, 1);

            double deviation = fiboMultiplier * currentDeviation;

            double fu764 = currentBasis + (0.001 * fiboLevel * deviation);
            double fd764 = currentBasis - (0.001 * fiboLevel * deviation);

            //TODO: Maybe remove deviation multiplier (applied before)
            double fu1 = currentBasis + (1 * deviation);
            double fd1 = currentBasis - (1 * deviation);

            //Max fibo level
            this.SetIndicatorValue(fu1, 1);

            //Upper fibo level
            this.SetIndicatorValue(fu764, 2);

            //VWMA (Basis)
            this.SetIndicatorValue(currentBasis, 3);

            //Lower fibo level
            this.SetIndicatorValue(fd764, 4);

            //Min fibo level
            this.SetIndicatorValue(fd1, 5);
        }

        #region Visual Chart Code

        /// <summary>
        /// Performs calculus between startBar and endBar.
        /// </summary>
        /// <param name="startBar">Initial calculus bar.</param>
        /// <param name="endBar">End calculus bar.</param>
        public override void OnCalculateRange(int startBar, int endBar)
        {
            int i = this.StartBar;
            if (startBar > i)
                i = startBar;

            while (!this.ShouldTerminate && i <= endBar)
            {
                this.CurrentBar = i;
                this.CalculateAggregators();
                this.OnCalculateBar(i);
                i++;
            }
        }

        /// <summary>
        /// Sets calculus parameters.
        /// </summary>
        /// <param name="paramList">Parameters list.</param>
        public override void OnSetParameters(List<object> paramList)
        {
            fiboLength = Convert.ToInt32(paramList[0]);
            fiboLevel = Convert.ToInt32(paramList[1]);
            fiboMultiplier = Convert.ToDouble(paramList[2]);
        }

        /// <summary>
        /// This function is used to create the data series corresponding to any indicator and to obtain an identifier of this series. To do so, we need to declare first a variable DataIdentifier type. Once the variable is defined we will always assign to it the the value of the function GetIndicatorIdentifier in order to create the indicator data series and to obtain an identifier of the same indicator. The identifier of the indicator must be obtained from the procedure <see cref="OnInitCalculate"/>.
        /// Later on, in order to obtain the value of an indicator we must use the functionGetIndicatorValue and indicate in the parameter Data the variable on which we have saved the value of the corresponding indicator. The identifier obtained by this function can be use don any .NET function on which a Data is required (Data series on which the different functions are calculated).
        /// </summary>
        /// <param name="indicator">Indicator Id.</param>
        /// <param name="parentDataIdentifier">Identifier of the series on which the indicator is calculated. If we set this parameter as data, we will be calculating the indicator on the data or series on which the strategy is being calculated. If we are willing to obtain the identifier of an indicator being calculated on another indicator we shall indicate within this parameter the identifier of the indicator we are willing to use as calculation.</param>
        /// <param name="optionalParameters">Indicator parameters (can be null).</param>
        /// <returns>Indicator source identifier.</returns>        
        public DataIdentifier GetIndicatorIdentifier(Indicators indicator, DataIdentifier parentDataIdentifier, params object[] optionalParameters)
        {
            return base.GetIndicatorIdentifier((long)indicator, parentDataIdentifier, optionalParameters);
        }

        /// <summary>
        /// This function is used to create the data series corresponding to any indicator and to obtain an identifier of this series. To do so, we need to declare first a variable DataIdentifier type. Once the variable is defined we will always assign to it the the value of the function GetIndicatorIdentifier in order to create the indicator data series and to obtain an identifier of the same indicator. The identifier of the indicator must be obtained from the procedure <see cref="OnInitCalculate"/>.
        /// Later on, in order to obtain the value of an indicator we must use the functionGetIndicatorValue and indicate in the parameter Data the variable on which we have saved the value of the corresponding indicator. The identifier obtained by this function can be use don any .NET function on which a Data is required (Data series on which the different functions are calculated).
        /// </summary>
        /// <param name="indicator">Indicator Id.</param>
        /// <param name="parentDataIdentifier">Identifier of the series on which the indicator is calculated. If we set this parameter as data, we will be calculating the indicator on the data or series on which the strategy is being calculated. If we are willing to obtain the identifier of an indicator being calculated on another indicator we shall indicate within this parameter the identifier of the indicator we are willing to use as calculation.</param>
        /// <param name="optionalParameters">Indicator parameters (can be null).</param>
        /// <returns>Indicator source identifier.</returns> 
        public DataIdentifier GII(Indicators indicator, DataIdentifier parentDataIdentifier, params object[] optionalParameters)
        {
            return base.GII((long)indicator, parentDataIdentifier, optionalParameters);
        }

        /// <summary>
        /// This function enables to obtain internally, the information of a certain system. This way, we can extract the information from this system without having to calculate it once and once again.
        /// </summary>
        /// <param name="strategy">Strategy id.</param>
        /// <param name="parentDataIdentifier">Identifier of the series on which the system is calculated. If we set this parameter as data, we will be calculating the indicator on the data or series on which the strategy is being calculated. If we are willing to obtain the identifier of an indicator being calculated on another indicator we shall indicate within this parameter the identifier of the indicator we are willing to use as calculation.</param>
        /// <param name="optionalParameters">System parameters (can be null).</param>
        /// <returns>system source identifier.</returns>
        public DataIdentifier GetSystemIdentifier(Strategies strategy, DataIdentifier parentDataIdentifier, params object[] optionalParameters)
        {
            return base.GetSystemIdentifier((long)strategy, parentDataIdentifier, optionalParameters);
        }

        /// <summary>
        /// This function enables to obtain internally, the information of a certain system. This way, we can extract the information from this system without having to calculate it once and once again.
        /// </summary>
        /// <param name="strategy">Strategy id.</param>
        /// <param name="parentDataIdentifier">Identifier of the series on which the system is calculated. If we set this parameter as data, we will be calculating the indicator on the data or series on which the strategy is being calculated. If we are willing to obtain the identifier of an indicator being calculated on another indicator we shall indicate within this parameter the identifier of the indicator we are willing to use as calculation.</param>
        /// <param name="optionalParameters">System parameters (can be null).</param>
        /// <returns>system source identifier.</returns>
        public DataIdentifier GSYSI(Strategies strategy, DataIdentifier parentDataIdentifier, params object[] optionalParameters)
        {
            return base.GSYSI((long)strategy, parentDataIdentifier, optionalParameters);
        }

        /// <summary>
        /// This function is used to paint the background of the window, for a certain bar, in the indicated color.
        /// </summary>    
        /// <param name="barsAgo">Number of bars backwards.The value 0 refers to the current bar.</param>
        /// <param name="color">Color used for the background on the given bar(BarsAgo).</param>
        public void SetBackgroundColor(int barsAgo, Color color)
        {
            this.SetBackgroundColor(barsAgo, Color.FromArgb(0, color.B, color.G, color.R).ToArgb());
        }

        /// <summary>
        /// Assigns the background color to the window of an indicator.
        /// </summary>
        /// <param name="color">Use of the function RGB to identify the background color of a window.</param>
        public void SetWndBackgroundColor(Color color)
        {
            this.SetWndBackgroundColor(Color.FromArgb(0, color.B, color.G, color.R).ToArgb());
        }

        /// <summary>
        /// This function assigns to the indicated bar of a certain indicator line, a certain color.
        /// </summary>
        /// <param name="barsAgo">Number of bars backwards.The value 0 refers to the current bar.</param>
        /// <param name="line">Identifies the data line to which the bar on which the property is established belongs.</param>
        /// <param name="color">Color to paint the indicated bar.</param>
        public void SetBarColor(int barsAgo, int line, Color color)
        {
            this.SetBarColor(barsAgo, line, Color.FromArgb(0, color.B, color.G, color.R).ToArgb());
        }

        /// <summary>
        /// This function assigns to the indicated bar, of a certain line of the indicator, the color, width and type of line and also the representation used in the rest of the parameters.
        /// </summary>
        /// <param name="barsAgo">Number of bars backwards.The value 0 refers to the current bar.</param>
        /// <param name="line">Identifies the data line to which the bar on which the property is established belongs.</param>
        /// <param name="color">Color to paint the indicated bar. FunctionRGB.</param>
        /// <param name="width">Indicates the width to be applied to the bar (1,2,..).</param>
        /// <param name="style">Style used for the representation: <see cref="LineStyle.Solid"/>-> continuous line <see cref="LineStyle.Dash"/>-> non-continuous line <see cref="LineStyle.Dot"/>-> dotted line <see cref="LineStyle.Dashdot"/>-> dotted line with point <see cref="LineStyle.Dashdotdot"/>-> dotted line with 2 points.</param>
        /// <param name="representation">Type or representation bo be used: <see cref="IndicatorRepresentation.Bars"/>-> bars <see cref="IndicatorRepresentation.Candlestic"/>-> candlesticks <see cref="IndicatorRepresentation.DottedLine"/>-> dotted line <see cref="IndicatorRepresentation.FilledHistogram"/>-> filled histogram <see cref="IndicatorRepresentation.Histogram"/>-> histogram <see cref="IndicatorRepresentation.Lineal"/>Lineal-> lineal <see cref="IndicatorRepresentation.Parabolic"/>-> Parabolic <see cref="IndicatorRepresentation.Volume"/>-> volume.</param>
        public void SetBarProperties(int barsAgo, int line, Color color, int width, LineStyle style, IndicatorRepresentation representation)
        {
            this.SetBarProperties(barsAgo, line, Color.FromArgb(0, color.B, color.G, color.R).ToArgb(), width, style, representation);
        }

        #endregion
    }
}

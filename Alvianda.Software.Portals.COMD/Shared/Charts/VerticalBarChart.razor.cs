using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using ChartJs.Blazor.Charts;
using ChartJs.Blazor.ChartJS.PieChart;
using ChartJs.Blazor.ChartJS.BarChart;
using ChartJs.Blazor.ChartJS.Common.Properties;
using ChartJs.Blazor.ChartJS.Common.Enums;
using TData = ChartJs.Blazor.ChartJS.Common.Wrappers;
using ChartJs.Blazor.Util;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using ChartJs.Blazor.ChartJS.Common;
using System.Security.Cryptography.X509Certificates;
using ChartJs.Blazor.ChartJS.BubbleChart;
using ChartJs.Blazor.ChartJS.BarChart.Axes;
using ChartJs.Blazor.ChartJS.Common.Axes;
using ChartJs.Blazor.ChartJS.Common.Axes.Ticks;

namespace DH.StudentLending.Portals.COMP.Shared.Charts
{
    public partial class VerticalBarChart
    {
        protected BarConfig _barConfig;
        protected ChartJsBarChart _barChartJs;


    }
}

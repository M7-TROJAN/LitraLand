// apexcharts for the rentals chart https://apexcharts.com/docs/installation/
// chart.js for the subscribers chart https://www.chartjs.org/docs/latest/

var chart; // Global variable to store the chart instance (to be able to destroy it when the date range changes)

function drawRentalsChart(startDate = null, endDate = null) {
    var element = document.getElementById('RentalsPerDay');

    var height = parseInt(element.offsetHeight);
    var labelColor = '#A1A5B7'; //gray-500
    var borderColor = '#eff2f5'; //gray-200
    var baseColor = '#7239ea'; //info
    var lightColor = '#f8f5ff'; //info-light

    if (!element)
        return;

    $.get({
        url: `/Library/Dashboard/GetRentalsPerDay?startDate=${startDate}&endDate=${endDate}`,
        success: function (returnedData) {

            var options = {
                series: [{
                    name: 'Books',
                    data: returnedData.map(i => i.value)
                }],
                chart: {
                    fontFamily: 'inherit',
                    type: 'area',
                    height: height,
                    toolbar: {
                        show: false
                    }
                },
                plotOptions: {

                },
                legend: {
                    show: false
                },
                dataLabels: {
                    enabled: false
                },
                fill: {
                    type: 'solid',
                    opacity: 1
                },
                stroke: {
                    curve: 'smooth',
                    show: true,
                    width: 3,
                    colors: [baseColor]
                },
                xaxis: {
                    categories: returnedData.map(i => i.label),
                    axisBorder: {
                        show: false,
                    },
                    axisTicks: {
                        show: false
                    },
                    labels: {
                        style: {
                            colors: labelColor,
                            fontSize: '12px'
                        }
                    },
                    crosshairs: {
                        position: 'front',
                        stroke: {
                            color: baseColor,
                            width: 1,
                            dashArray: 3
                        }
                    },
                    tooltip: {
                        enabled: true,
                        formatter: undefined,
                        offsetY: 0,
                        style: {
                            fontSize: '12px'
                        }
                    }
                },
                yaxis: {
                    tickAmount: Math.max(...returnedData.map(d => d.value)),
                    min: 0,
                    labels: {
                        style: {
                            colors: labelColor,
                            fontSize: '12px'
                        }
                    }
                },
                states: {
                    normal: {
                        filter: {
                            type: 'none',
                            value: 0
                        }
                    },
                    hover: {
                        filter: {
                            type: 'none',
                            value: 0
                        }
                    },
                    active: {
                        allowMultipleDataPointsSelection: false,
                        filter: {
                            type: 'none',
                            value: 0
                        }
                    }
                },
                tooltip: {
                    style: {
                        fontSize: '12px'
                    }
                },
                colors: [lightColor],
                grid: {
                    borderColor: borderColor,
                    strokeDashArray: 4,
                    yaxis: {
                        lines: {
                            show: true
                        }
                    }
                },
                markers: {
                    strokeColor: baseColor,
                    strokeWidth: 3
                }
            };

            chart = new ApexCharts(element, options);
            chart.render();
        }
    });
}

function drawSubscribersChart() {
    $.get({
        url: '/Library/Dashboard/GetSubscribersPerCity',
        success: function (figures) {
            var ctx = document.getElementById('SubscribersPerCity'); // ctx means context and it's the canvas element where the chart will be drawn on

            // Define colors
            var primaryColor = '#009ef7'; // primary 
            var dangerColor = '#f1416c'; // danger
            var successColor = '#50cd89'; // success
            var warningColor = '#ffc700'; // warning
            var infoColor = '#7239ea' //info

            // Define fonts
            var fontFamily = 'Inter,Helvetica,"sans-serif"';

            // Chart data
            const data = {
                labels: figures.map(f => f.label),
                datasets: [{
                    data: figures.map(f => f.value),
                    backgroundColor: [
                        infoColor,
                        successColor,
                        warningColor,
                        primaryColor,
                        dangerColor,
                        '#5F91B6',
                        '#D3F6FC',
                        '#C8B0D2'
                    ],
                    borderRadius: 8
                }]
            };

            // Chart config
            const config = {
                type: 'doughnut',
                data: data,
                options: {
                    plugins: {
                        title: {
                            display: false,
                        }
                    },
                    responsive: true,
                },
                defaults: {
                    global: {
                        defaultFont: fontFamily
                    }
                }
            };

            new Chart(ctx, config);
        }
    });
}

document.addEventListener("DOMContentLoaded", function () {

    // old code of the date range picker event listener (not working)
    // because  'DOMSubtreeModified' is deprecated and removed from the web standards and it's not supported by all browsers
    /*
    $('#DateRange').on('DOMSubtreeModified', function () {
        var selectedRange = $(this).html();
        console.log(selectedRange);

        if (selectedRange !== '') {
            var dateRange = selectedRange.split(' - ');
            chart.destroy();
            drawRentalsChart(dateRange[0], dateRange[1]);
        }
    });
    */

    // New code using MutationObserver API to detect changes in the element content
    // https://developer.mozilla.org/en-US/docs/Web/API/MutationObserver 

    let targetNode = document.getElementById('DateRange');

    if (!targetNode) {
        console.error("Element #DateRange not found!");
        return;
    }

    let observer = new MutationObserver(function (mutationsList) {
        for (let mutation of mutationsList) {
            if (mutation.type === 'childList' || mutation.type === 'characterData') {
                let selectedRange = targetNode.innerText.trim();
                console.log("New Date Range:", selectedRange);

                if (selectedRange !== '') {
                    let dateRange = selectedRange.split(' - ');

                    if (dateRange.length === 2) {
                        let startDate = dateRange[0].trim();
                        let endDate = dateRange[1].trim();

                        if (window.chart)
                            window.chart.destroy();

                        drawRentalsChart(startDate, endDate);
                    }
                    else if (dateRange.length === 1) {
                        let startDate = dateRange[0].trim();
                        let endDate = dateRange[0].trim();
                        if (window.chart)
                            window.chart.destroy();
                        drawRentalsChart(startDate, endDate);
                    }
                }
            }
        }
    });

    // Initialize `MutationObserver` to monitor changes in the target element
    observer.observe(targetNode, {
        childList: true,  // Watch for changes in child elements (HTML elements inside the div)
        subtree: true,    // Watch for changes in all nested elements (sub-elements) div > div > div > ...
        characterData: true // Watch for direct text changes inside the element (not in child elements just the text inside the div who has the observer)
    });
});

drawRentalsChart();
drawSubscribersChart();
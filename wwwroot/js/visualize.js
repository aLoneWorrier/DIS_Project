function displaychart() {
    var gID = $('#genres').val();
    let f = new FormData();
    f.append("id", gID);
    $.ajax({
        method: "POST",
        url: "/Movie/Model",
        cache: false,
        contentType: false,
        processData: false,
        data: f,
        success: function (chartdata) {
            let ratings = chartdata[0];
            let movieCount = chartdata[1];
            let genreName = chartdata[2];
            if (genreName == null) {
                genreName = "All Genres"
            }
            let ctx = $("#barchart").get(0).getContext("2d");
            if (window.bar != undefined)
                window.bar.destroy(); 
            window.bar = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: ratings,
                    datasets: [
                        {
                            label: "Count of Movies",
                            backgroundColor: Array(10).fill(["#3e95cd", "#8e5ea2", "#3cba9f", "#e8c3b9", "#c45850"]).flat(),
                            data: movieCount
                        }
                    ]
                },
                options: {
                    responsive: true,
                    legend: { display: false },
                    title: {
                        display: true,
                        text: 'IMDB Rating - wise count of movies for ' + genreName
                    },
                    scales: {
                        xAxes: [{
                            ticks: {
                                autoSkip: true,
                                minRotation: 90,
                                maxRotation: 90
                            }
                        }],
                        yAxes: [{
                            ticks: {
                                beginAtZero: true,
                                stepSize: 5,
                                precision: 0
                            }
                        }]
                    }
                }
            });
        },
        error: function (req, status, error) {
            console.log(error);
        }
    });
}

$('#genres').change(displaychart);
$(document).ready(displaychart);





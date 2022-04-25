var elems = document.querySelectorAll('.carousel-slider');
var instances = M.Carousel.init(elems, {
    duration: 200,
    indicators: true,
    fullWidth: true
});
setInterval(function () { instances[0].next(); }, 4000);
var gulp = require('gulp');
//var autoprefixer = require('autoprefixer');
//var cssnano = require('cssnano');
//var postcss = require('gulp-postcss');
var concat = require('gulp-concat');
var uglify = require('gulp-uglify');
//var sourcemaps = require('gulp-sourcemaps');
//var sass = require('gulp-sass')

/*var sassOptions = {
  outputStyle: 'expanded'
};*/

gulp.task('default', function (done) {
    /*var cssplugins = [
        autoprefixer(),
        cssnano()
    ];
    gulp.src('input/assets/css/main.scss')
        .pipe(sourcemaps.init())
        .pipe(sass(sassOptions))
        .pipe(gulp.src('input/assets/css/custom-icons.css'))
        .pipe(concat('all.min.css'))
        .pipe(postcss(cssplugins))
        .pipe(sourcemaps.write('maps'))
        .pipe(gulp.dest('input/assets/css/'));*/
    gulp.src(['input/assets/js/shared.js'], { sourcemaps: true })
        .pipe(concat('all.min.js'))
        .pipe(uglify())
        .pipe(gulp.dest('input/assets/js/', { sourcemaps: true }));
    gulp.src(['input/assets/js/finalizeAwsSale.js'], { sourcemaps: true })
        .pipe(concat('finalizeAwsSale.min.js'))
        .pipe(uglify())
        .pipe(gulp.dest('input/assets/js/', { sourcemaps: true }));
    gulp.src(['input/assets/js/paintingPage.js'], { sourcemaps: true })
        .pipe(concat('paintingPage.min.js'))
        .pipe(uglify())
        .pipe(gulp.dest('input/assets/js/', { sourcemaps: true }));
    gulp.src(['input/assets/js/awsPurchase.js'], { sourcemaps: true })
        .pipe(concat('awsPurchase.min.js'))
        .pipe(uglify())
        .pipe(gulp.dest('input/assets/js/', { sourcemaps: true }));
    gulp.src(['input/assets/js/paypalPurchase.js'], { sourcemaps: true })
        .pipe(concat('paypalPurchase.min.js'))
        .pipe(uglify())
        .pipe(gulp.dest('input/assets/js/', { sourcemaps: true }));
    gulp.src(['input/assets/js/finalizePaypalSale.js'], { sourcemaps: true })
        .pipe(concat('finalizePaypalSale.min.js'))
        .pipe(uglify())
        .pipe(gulp.dest('input/assets/js/', { sourcemaps: true }));
    done();
});

gulp.task('watch', function () {
    gulp.watch(['input/assets/css/*', 'input/assets/js/*', '!input/assets/css/*.min.css', '!input/assets/js/*.min.js'], gulp.series('default'));
})

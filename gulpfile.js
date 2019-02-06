var gulp = require('gulp');
var cssmin = require('gulp-cssmin');
var concat = require('gulp-concat');
var uglify = require('gulp-uglify');
var sourcemaps = require('gulp-sourcemaps');

gulp.task('default', function (done) {
    gulp.src(['input/assets/css/main.css', 'input/assets/css/custom-icons.css'])
	    .pipe(concat('all.min.css'))
        .pipe(cssmin())
        .pipe(gulp.dest('input/assets/css/'));
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
    done();
});

var gulp = require('gulp');
var cssmin = require('gulp-cssmin');
var concat = require('gulp-concat');
var uglify = require('gulp-uglifyjs');

gulp.task('default', function () {
    gulp.src(['contents/css/main.css', 'contents/css/responsive-nav.css', 'contents/css/custom-icons.css'])
	.pipe(concat('all.min.css'))
        .pipe(cssmin())
        .pipe(gulp.dest('contents/css/'));
    gulp.src(['contents/js/jquery-2.1.0.min.js', 'contents/js/responsive-nav.min.js'])
        .pipe(uglify('all.min.js', {outSourceMap: true}))
        .pipe(gulp.dest('contents/js/'));
});

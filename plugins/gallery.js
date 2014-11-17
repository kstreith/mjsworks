var __hasProp = {}.hasOwnProperty,
  __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

module.exports = function(env, callback) {

  /* Paginator plugin. Defaults can be overridden in config.json
      e.g. "paginator": {"perPage": 10}
   */
  var GalleryPage, getGalleries, key, options, value;
  options = {
    template: 'index.jade',
    articles: 'articles',
    first: 'index.html',
    filename: 'page/%d/index.html',
    perPage: 2
  };
  getGalleries = function(contents) {
    console.log('here2');
    console.log('here3 ' + Object.keys(contents));
    console.log(Object.keys(contents.gallery));
    var galleries = [];
    Object.keys(contents.gallery).forEach(function(item) {
      console.log('here4 ' + item);
      //assume one series per gallery
      var series = contents.gallery[item].metadata.series[0];
      var seriesTitle = series.title;
      var seriesPhotos = series.photos;
      var gallery = {title: seriesTitle, photos: []};
      galleries.push(gallery); 
      seriesPhotos.forEach(function (seriesPhoto) {
        gallery.photos.push({title:seriesPhoto.title, file:seriesPhoto.file, gallery:seriesTitle});
      });
    });
    return galleries;
  };
  var getGalleryFile = function (galleryTitle, galleryImageTitle) {
      return 'gallery/' + galleryTitle + '/' + galleryImageTitle + '.html';
  }
  GalleryPage = (function(_super) {
    __extends(GalleryPage, _super);


    /* A page has a number and a list of articles */

    function GalleryPage(galleryTitle, galleryImage, previous, next) {
      this.galleryTitle = galleryTitle;
      this.galleryImage = galleryImage;
      this.previous = previous;
      this.next = next;
    }

    GalleryPage.prototype.getFilename = function() {
      return getGalleryFile(this.galleryTitle, this.galleryImage.title);
    };

    GalleryPage.prototype.getView = function() {
      var self = this;
      console.log('here22 ' + JSON.stringify(self.galleryImage));
      return function(env, locals, contents, templates, callback) {
        var ctx, template;
        template = templates["gallery.html"];
        if (template == null) {
          return callback(new Error("unknown gallery template 'gallery.html'"));
        }
        ctx = {
          galleryImage: self.galleryImage,
          nextImage: self.next,
          previousImage: self.previous
        };
        env.utils.extend(ctx, locals);
        return template.render(ctx, callback);
      };
    };

    return GalleryPage;

  })(env.plugins.Page);
  env.registerGenerator('paginator', function(contents, callback) {
    var galleries, i, numPages, page, pageArticles, pages, rv, _i, _j, _k, _len, _len1;
    galleries = getGalleries(contents);
    pages = [];
    rv = {
      gallery: {},
    }
    galleries.forEach(function (gallery) {
      var curGallery = {};
      rv.gallery[gallery.title] = curGallery;
      gallery.photos.forEach(function (photo, index, photoList) {
        var previous = photoList[photoList.length - 1].title;
        var next = photoList[0].title;
        if (index > 0) {
          previous = photoList[index - 1].title;
        }
        if (index < (photoList.length - 1)) {
          next = photoList[index + 1].title;
        }
        curGallery[photo.title] = new GalleryPage(gallery.title, photo, previous, next); 
      });
    });
    return callback(null, rv);
  });
  env.helpers.getGalleries = getGalleries;
  return callback();
};

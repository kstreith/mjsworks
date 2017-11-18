var imageSize = require('image-size');
var __hasProp = {}.hasOwnProperty,
  __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

function slugify(text)
{
  return text.toString().toLowerCase()
    .replace(/\s+/g, '-')           // Replace spaces with -
    .replace(/[^\w\-]+/g, '')       // Remove all non-word chars
    .replace(/\-\-+/g, '-')         // Replace multiple - with single -
    .replace(/^-+/, '')             // Trim - from start of text
    .replace(/-+$/, '');            // Trim - from end of text
}

module.exports = function(env, callback) {

  var GalleryPage, getGalleries;
  parseGallery = function (metadata, galleryName, galleries) {
    var template = metadata['template'];
    var navSection = 'unknown';
    if (template == 'paintinggallery.html') {
      navSection = 'painting'
    }
    if (template == "feathers.html") {
      navSection = "feathers"
    }
    var series = metadata['series']
    if (series.length !== 1) {
      throw new Error("Gallery can only contain one series");
    }
    var firstSeries = series[0]
    //assume one series per gallery
    var seriesPhotos = firstSeries.photos;
    var gallery = {title: galleryName, photos: [], navSection: navSection};
    galleries.push(gallery); 
    seriesPhotos.forEach(function (seriesPhoto) {
      if (seriesPhoto.file.toLowerCase().endsWith(".jpg")) {
        mimeType = "image/jpeg";
      } else {
        throw new Error("Unknown file type");
      }
      var description = seriesPhoto.description || env.locals.defaultImageDescription;
      gallery.photos.push({
        title:seriesPhoto.title,
        slug:slugify(seriesPhoto.title),
        file:seriesPhoto.file,
        gallery:galleryName,
        sold:seriesPhoto.sold,
        size:seriesPhoto.size,
        mimeType: mimeType,
        description: description
      });
    });  
  }
  getGalleries = function(contents) {
    var galleries = [];
    console.log('galleries');
    Object.keys(contents.gallery).forEach(function(galleryName) {
      var theGallery = contents.gallery[galleryName];
      var index = theGallery['index.json'];
      var metadata = index['metadata'];
      parseGallery(metadata, galleryName, galleries);
    });
    return galleries;
  };
  getFeatherGallery = function(contents, galleries)
  {
    console.log('feather gallery');
    var feathersJson = contents["feathers.json"]
    var metadata = feathersJson["metadata"]
    parseGallery(metadata, "feathers", galleries)
  }
  var getGalleryFile = function (galleryTitle, galleryImageTitle) {
      return 'gallery/' + galleryTitle + '/' + galleryImageTitle + '.html';
  }
  var getGalleryLink = function (galleryTitle) {
    if (galleryTitle == "feathers") {
      return "/feathers.html";
    }
    return '/gallery/' + galleryTitle + '/';    
  }
  GalleryPage = (function(_super) {
    __extends(GalleryPage, _super);


    /* A page has a number and a list of articles */

    function GalleryPage(galleryTitle, galleryImage, previous, next, navSection) {
      this.galleryTitle = galleryTitle;
      this.galleryImage = galleryImage;
      this.previous = previous;
      this.next = next;
      this.navSection = navSection;
    }

    GalleryPage.prototype.getFilename = function() {
      var filename = getGalleryFile(this.galleryTitle, this.galleryImage.slug);
      //console.log('here44' + filename);
      return filename;
    };

    GalleryPage.prototype.getView = function() {
      var self = this;
      //console.log('here22 ' + JSON.stringify(self.galleryImage));
      var imagePath = "contents/photos/images/" + self.galleryImage.file;
      //console.log('here77 ' + imagePath);
      var imageDimensions = imageSize(imagePath);
      //console.log('here88 ' + JSON.stringify(imageDimensions));
      return function(env, locals, contents, templates, callback) {
        var ctx, template;
        template = templates["gallery.html"];
        if (template == null) {
          return callback(new Error("unknown gallery template 'gallery.html'"));
        }
        ctx = {
          galleryImage: self.galleryImage,
          nextImage: self.next,
          previousImage: self.previous,
          galleryLink: getGalleryLink(self.galleryTitle),
          navSection: self.navSection,
          imageDimensions: imageDimensions
        };
        env.utils.extend(ctx, locals);
        return template.render(ctx, callback);
      };
    };

    return GalleryPage;

  })(env.plugins.Page);
  env.registerGenerator('gallery', function(contents, callback) {
    var galleries, i, numPages, page, pageArticles, pages, rv, _i, _j, _k, _len, _len1;
    galleries = getGalleries(contents);
    getFeatherGallery(contents, galleries);
    pages = [];
    rv = {
      gallery: {},
    }
    galleries.forEach(function (gallery) {
      var curGallery = {};
      rv.gallery[gallery.title] = curGallery;
      gallery.photos.forEach(function (photo, index, photoList) {
        var previous = photoList[photoList.length - 1].slug;
        var next = photoList[0].slug;
        if (index > 0) {
          previous = photoList[index - 1].slug;
        }
        if (index < (photoList.length - 1)) {
          next = photoList[index + 1].slug;
        }
        curGallery[photo.slug] = new GalleryPage(gallery.title, photo, previous, next, gallery.navSection); 
      });
    });
    return callback(null, rv);
  });
  env.helpers.getGalleries = getGalleries;
  env.helpers.slug = function (title) {
    return slugify(title);
  }
  return callback();
};

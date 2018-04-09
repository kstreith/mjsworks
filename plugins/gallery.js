var imageSize = require('image-size');
var crypto = require('crypto');
querystring = require('querystring')
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

function generateSignature(dataToSign) {
  var sharedSecret = getEnvironmentVariable("MJS_STATIC_GENERATION_SIGNING_KEY");
  console.log('dataToSign', dataToSign);
  console.log('sharedSecret', sharedSecret);
  var signature = crypto.createHmac("sha256", sharedSecret).update(dataToSign).digest("base64");
  return signature;  
}
function getEnvironmentVariable(name) {
  var value = process.env[name];
  if (!value) {
    throw new Error("Please set the " + name + " environment variable");
  }
  return value;
}
function formatPrice(priceNumber) {
  if (priceNumber > 0) {
    var priceString = priceNumber.toFixed(2);
    return priceString;
  }
  return "";
}

module.exports = function(env, callback) {  
  var GalleryPage, getGalleries;
  function encode(str) {
    return querystring.escape(str);
  }
  function generateAwsButton(amount, sellerNote, sellerOrderId) {
    var publicAccessKey = env.locals.awsPublicAccessKey;
    var sellerId = env.locals.awsSellerId;
    var lwaClientId = env.locals.awsLwaClientId;
    var returnURL = getEnvironmentVariable("MJS_WORKS_SITE_HOST_SECURE") + "finalizedSale.html";
    var dict = {};
    dict["shippingAddressRequired"] = "true";
    dict["currencyCode"] = "USD";
    dict["paymentAction"] = "Authorize";
    dict["accessKey"] = publicAccessKey;
    dict["amount"] = amount;
    dict["lwaClientId"] = lwaClientId;
    dict["returnURL"] = returnURL;
    dict["sellerId"] = sellerId;
    dict["sellerNote"] = sellerNote;
    dict["sellerOrderId"] = sellerOrderId;
    
    var urlToSign = "static-site-generation:mjsworks:";
    for (var key of Object.keys(dict).sort()) {
      urlToSign += encode(key) + "=" + encode(dict[key]) + "&";
    }
    urlToSign = urlToSign.slice(0, -1);
    var signature =  generateSignature(urlToSign);

    dict["static_generation_signature"] = signature;
    return JSON.stringify(dict);
  }
    
  shouldShowSellUi = function(seriesPhoto) {
    return seriesPhoto.forSale && !seriesPhoto.sold;
  }
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
      var customDescription = seriesPhoto.description;
      var showSellUi = shouldShowSellUi(seriesPhoto);
      if (showSellUi) {
        //validation
        if (!seriesPhoto.size) {
          throw new Error(seriesPhoto.file + " missing size which is required");
        }
        var sku = seriesPhoto.sku;
        if (!sku) {
          throw new Error(seriesPhoto.file + " missing sku which is required");
        }
        if (sku.length == 7) {
          if (!(sku.startsWith('p') && sku.endsWith('e'))) {
            throw new Error(seriesPhoto.file + " sku is something other than encaustic painting");
          }
        }
        else {
          throw new Error(seriesPhoto.file + " sku is wrong length");          
        }
        if (!description) {
          throw new Error(seriesPhoto.file + " missing description which is required");
        }
        if (seriesPhoto.price <= 0) {
          throw new Error(seriesPhoto.file + " missing price which is required");
        }        
      }
      customDescription = customDescription || env.locals.defaultImageDescription;
      var sellerNote = "";
      var type = "";
      //encaustic specific section
      description = customDescription;
      saleDescription = customDescription + " Original encaustic painting on wood birch panel with wire hanger";
      sellerNote = "Original encaustic painting";
      type = "encaustic-painting"
      //end encaustic specific section
      if (description.length > 1024) {
        var amountOver = description.length - 1024;
        var overText = description.slice(description.length - amountOver);
        throw new Error(seriesPhoto.file + " description is too long, following text goes over limit " + overText);
      }
      gallery.photos.push({
        title:seriesPhoto.title,
        slug:slugify(seriesPhoto.title),
        file:seriesPhoto.file,
        gallery:galleryName,
        sold:seriesPhoto.sold,
        showSellUi:showSellUi,
        sellUiCssClass:showSellUi ? "sellUi" : "",
        sku:seriesPhoto.sku,
        size:seriesPhoto.size,
        type:type,
        price:formatPrice(seriesPhoto.price),
        mimeType: mimeType,
        description: description,
        saleDescription: saleDescription,
        sellerNote: sellerNote
      });
    });  
  }
  getGalleries = function(contents) {
    var galleries = [];
    console.log('galleries');
    Object.keys(contents.gallery).forEach(function(galleryName) {
      var theGallery = contents.gallery[galleryName];
      var index = theGallery['index.json'] || theGallery['index.yaml'];
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

    function GalleryPage(galleryTitle, galleryImage, previous, next, navSection) {
      this.galleryTitle = galleryTitle;
      this.galleryImage = galleryImage;
      this.previous = previous;
      this.next = next;
      this.navSection = navSection;
    }

    GalleryPage.prototype.getFilename = function() {
      var filename = getGalleryFile(this.galleryTitle, this.galleryImage.slug);
      return filename;
    };

    GalleryPage.prototype.getView = function() {
      var self = this;
      var imagePath = "contents/photos/images/" + self.galleryImage.file;
      var imageDimensions = imageSize(imagePath);
      var awsButtonJson = generateAwsButton(self.galleryImage.price, self.galleryImage.sellerNote, self.galleryImage.sku);
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
          imageDimensions: imageDimensions,
          awsButtonJson: awsButtonJson,
          awsWidgetsUrl:getEnvironmentVariable("MJS_WORKS_AWS_WIDGETS_URL")
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
    var skuDict = {};
    var duplicatedSkus = [];
    galleries.forEach(function (gallery) {
      var curGallery = {};
      rv.gallery[gallery.title] = curGallery;
      gallery.photos.forEach(function (photo, index, photoList) {
        var previous = photoList[photoList.length - 1].slug;
        var next = photoList[0].slug;
        var sku = photo.sku;
        if (sku) {
          if (skuDict.hasOwnProperty(sku)) {
            duplicatedSkus.push(sku);
          }
          skuDict[sku] = true;
        }
        if (index > 0) {
          previous = photoList[index - 1].slug;
        }
        if (index < (photoList.length - 1)) {
          next = photoList[index + 1].slug;
        }
        curGallery[photo.slug] = new GalleryPage(gallery.title, photo, previous, next, gallery.navSection); 
      });
    });
    if (duplicatedSkus.length) {
      console.log("Duplicated skus: " + duplicatedSkus.join());
    }
    var skus = Object.keys(skuDict);
    if (skus.length) {
      skus.sort();
      console.log('Most recent sku is ' + skus[skus.length - 1]);
    }
    return callback(null, rv);
  });
  env.helpers.formatPrice = formatPrice;
  env.helpers.shouldShowSellUi = shouldShowSellUi;
  env.helpers.slug = function (title) {
    return slugify(title);
  }
  env.helpers.getEnvironmentVariable = function(name) {
    return getEnvironmentVariable(name);
  }
  return callback();
};

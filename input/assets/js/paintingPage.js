(function() {
    var data = mjsworks.parseQuery();
    showGalleryNav(data["gallery"]);
    
    function setLink(selector, link) {
        var domNode = document.querySelector(selector);
        if (!domNode) {
            return;
        }
        domNode.href = link;        
    }

    function showGalleryNav(currentGallery) {
      if (!currentGallery) {
        return;
      }
      var matchedGallery = mjsworks.parentGalleries[currentGallery];
      if (!matchedGallery) {
        return;
      }
      var prevImage = matchedGallery.PreviousImage;
      var nextImage = matchedGallery.NextImage;
      setLink("#prevGalleryImageLink", "/paintings/" + prevImage + ".html?gallery=" + currentGallery);
      setLink("#nextGalleryImageLink", "/paintings/" + nextImage + ".html?gallery=" + currentGallery);
      setLink("#galleryHomeLink", "/gallery/" + currentGallery + "/index.html");
      mjsworks.show(".gallerynav");
    }
}());
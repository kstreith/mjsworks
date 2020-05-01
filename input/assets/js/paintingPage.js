(function() {
    var data = mjsworks.parseQuery();
    showGalleryNav(data["gallery"]);
    initializeSalesImageNavigation();

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

    var currentSalesImageIndex = 0;
    function initializeSalesImageNavigation() {
      var prevLink = document.querySelector("#prevSalesImageLink");
      if (prevLink) {
        prevLink.addEventListener("click", previousSalesImageClick);
      }
      var nextLink = document.querySelector("#nextSalesImageLink");
      if (nextLink) {
        nextLink.addEventListener("click", nextSalesImageClick);
      }
    }

    function updateSalesImage() {
      var salesImage = document.querySelector(".galleryimage > img");
      if (salesImage) {
        salesImage.src = mjsworks.salesImages[currentSalesImageIndex];
      }
      var salesImageIndex = document.querySelector("#salesImageIndex");
      if (salesImageIndex) {
        salesImageIndex.textContent = currentSalesImageIndex + 1;
      }
    }

    function previousSalesImageClick() {
      currentSalesImageIndex--;
      if (currentSalesImageIndex < 0) {
        currentSalesImageIndex = mjsworks.salesImages.length - 1;
      }
      updateSalesImage();
    }

    function nextSalesImageClick()
    {
      currentSalesImageIndex++;
      if (currentSalesImageIndex >= mjsworks.salesImages.length) {
        currentSalesImageIndex = 0;
      }
      updateSalesImage();
    }

}());
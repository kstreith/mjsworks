var __hasProp = {}.hasOwnProperty,
__extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

module.exports = function(env, callback) {
    var RedirectPage;
    RedirectPage = (function(_super) {
        __extends(RedirectPage, _super);
            
        function RedirectPage(missingFilename) {
            console.log('constructor ' + missingFilename + ' ' + this + ' ' + Object.keys(this))
          this.missingFilename = missingFilename;
        }
    
        RedirectPage.prototype.getFilename = function() {
            console.log('get filename ' + this + Object.keys(this));
          var self = this;
          return this.missingFilename;
        };
    
        RedirectPage.prototype.getView = function() {
          var self = this;
          //return "redirect-gone-page.html";
          return function(env, locals, contents, templates, callback) {
            var ctx, template;
            template = templates["redirect-gone-pages.html"];
            if (template == null) {
              return callback(new Error("unknown redirect gone template 'redirect-gone-page.html'"));
            }
            ctx = {
            };
            env.utils.extend(ctx, locals);
            return template.render(ctx, callback);
          };
        };
    
        return RedirectPage;
    
    })(env.ContentPlugin);
    env.registerGenerator('redirect-gone-pages', function(contents, callback) {
        rv = {
            gonepages: {},
        }
        rv.gonepages["gallery-atmosphere"] = new RedirectPage("gallery/atmosphere/index.html");
        rv.gonepages["gallery-painter-palette"] = new RedirectPage("gallery/painter-palette/index.html");
        rv.gonepages["gallery-plants"] = new RedirectPage("gallery/plants/index.html");
        rv.gonepages["gallery-sights"] = new RedirectPage("gallery/sights/index.html");
        rv.gonepages["gallery-trapped"] = new RedirectPage("gallery/trapped/index.html");
        rv.gonepages["painting-in-the-woods"] = new RedirectPage("gallery/dreamscapes/in-the-woods.html");
        rv.gonepages["painting-mighty-pen"] = new RedirectPage("gallery/mixed-media/mighty-pen.html");
        rv.gonepages["gallery-linear-study"] = new RedirectPage("gallery/linear-study/index.html");
        rv.gonepages["gallery-textures"] = new RedirectPage("gallery/textures/index.html");
        rv.gonepages["gallery-textures"] = new RedirectPage("gallery/flora/room-to-move.html");
        rv.gonepages["photos"] = new RedirectPage("photos.html");
        return callback(null, rv);
    });
    return callback();    
};    
    
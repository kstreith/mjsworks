"use strict";
var async = require('async');
var fs = require('fs');
var jsYaml = require('js-yaml');
var __hasProp = {}.hasOwnProperty,
__extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

module.exports = function(env, callback) {  
    var YamlPage;
    YamlPage = (function(_super) {
        __extends(YamlPage, _super);
    
        function YamlPage(filepath, metadata) {
            this.filepath = filepath;
            this.metadata = metadata;
            _super(filepath, metadata);
        }
        YamlPage.fromFile = function (filepath, callback) {
            return async.waterfall([
                function(callback) {
                  return fs.readFile(filepath.full, callback);
                },
                function(buffer, callback) {
                    try {
                        return callback(null, jsYaml.load(buffer) || {});
                    } catch (error1) {
                        var error = error1;
                        if ((error.problem != null) && (error.problemMark != null)) {
                            lines = error.problemMark.buffer.split('\n');
                            markerPad = ((function() {
                                var i, ref, results;
                                results = [];
                                for (i = 0, ref = error.problemMark.column; 0 <= ref ? i < ref : i > ref; 0 <= ref ? i++ : i--) {
                                    results.push(' ');
                                }
                                return results;
                            })()).join('');
                            error.message = `YAML: ${error.problem}\n\n${lines[error.problemMark.line]}\n${markerPad}^\n`;
                        } else {
                            error.message = `YAML Parsing error ${error.message}`;
                        }
                        return callback(error);
                    }                            
                },
                (metadata, callback) => {
                  //console.log('yaml parsed ' + JSON.stringify(metadata));
                  var page = new YamlPage(filepath, metadata, null);
                  return callback(null, page);
                }
              ], callback);
        };
        return YamlPage;         
    })(env.plugins.Page);    
    env.registerContentPlugin('pages', '**/*.yaml', YamlPage);
    return callback();    
}
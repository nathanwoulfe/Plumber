module.exports = function (grunt) {
    require('load-grunt-tasks')(grunt);
    require('time-grunt')(grunt);
    require('grunt-karma')(grunt);

    //cant load this with require
    grunt.loadNpmTasks('grunt-contrib-sass');
    grunt.loadNpmTasks('grunt-contrib-jshint');
    grunt.loadNpmTasks('grunt-contrib-cssmin');
    grunt.loadNpmTasks('grunt-banner');

    if (grunt.option('target') && !grunt.file.isDir(grunt.option('target'))) {
        grunt.fail.warn('The --target option specified is not a valid directory');
    }

    grunt.initConfig({
        packageVersion: function () {
            var buildVersion = grunt.option('buildversion') || '1.0.0.1',
                packageSuffix = grunt.option('packagesuffix') || 'build',
                buildBranch = grunt.option('buildbranch') || 'master';

            var findPoint = buildVersion.lastIndexOf('.');
            var basePackageVer = buildVersion.substring(0, findPoint);
            var buildNumber = buildVersion.substring(findPoint + 1, buildVersion.length);
            if (buildBranch.toLowerCase() !== 'release') {
                return basePackageVer + '-' + 'build' + buildNumber;
            } else if (packageSuffix !== 'build' && packageSuffix.length > 0) {
                return basePackageVer + '-' + packageSuffix;
            } else {
                return basePackageVer;
            }
        },
        pkg: grunt.file.readJSON('package.json'),
        dest: grunt.option('target') || '../dist',
        basePath: 'App_Plugins/workflow',
        banner:
            '*! <%= pkg.title || pkg.name %> - v<%= packageVersion() %> - <%= grunt.template.today("yyyy-mm-dd") %>\n' +
            '<%= pkg.homepage ? " * " + pkg.homepage + "\\n" : "" %>' +
            ' * Copyright (c) <%= grunt.template.today("yyyy") %> <%= pkg.author.name %>;\n' +
            ' * Licensed <%= pkg.license %>\n *',
        //Concat all the JS files into one
        concat: {
            dist: {
                src: [
                  '<%= basePath %>/backoffice/controllers/*.js',
                  '<%= basePath %>/backoffice/directives/*.js',
                  '<%= basePath %>/backoffice/interceptors/*.js',
                  '<%= basePath %>/backoffice/resources/*.js'
                ],
                dest: '<%= dest %>/<%= basePath %>/backoffice/js/workflow.js',
                nonull: true,
                options: {
                    banner: '/<%= banner %>/\n\n'
                }
            }
        },

        //Compile the less file into a CSS file
        sass: {
            dist: {
                files: {
                    '<%= basePath %>/backoffice/css/styles.css': ['<%= basePath %>/backoffice/css/styles.scss']
                },
            }
        },

        cssmin: {
            target: {
                files: [{
                    expand: true,
                    cwd: '<%= basePath %>/backoffice/css',
                    src: ['*.css'],
                    dest: '<%= dest %>/<%= basePath %>/backoffice/css',
                    ext: '.min.css'
                }]
            },
            add_banner: {
                files: {
                    '<%= dest %>/<%= basePath %>/backoffice/css/styles.min.css': ['<%= dest %>/<%= basePath %>/backoffice/css/styles.min.css']
                }
            }
        },

        watch: {
            dev: {
                files: ['<%= basePath %>/**/*.scss', '<%= basePath %>/**/*.js', '<%= basePath %>/**/*.html'],
                tasks: ['sass:dist', 'copy:css', 'copy:js', 'copy:html'],
                options: {
                    livereload: true
                }
            },

            css: {
                files: ['<%= basePath %>/**/*.scss'],
                tasks: ['sass:dist']
            },

            js: {
                files: ['<%= basePath %>/**/*.js'],
                tasks: ['concat:dist']
            },

            views: {
                files: ['<%= basePath %>/backoffice/views/**/*.html'],
                tasks: ['copy:views']
            },
            
            approvalGroups: {
                files: ['<%= basePath %>/backoffice/approval-groups/**/*.html'],
                tasks: ['copy:approval-groups']
            },
            
            partials: {
                files: ['<%= basePath %>/backoffice/partials/**/*.html'],
                tasks: ['copy:partials']
            },
            
            dialogs: {
                files: ['<%= basePath %>/backoffice/dialogs/**/*.html'],
                tasks: ['copy:dialogs']
            },
			
            groups: {
                files: ['<%= basePath %>/backoffice/approval-groups/**/*.html'],
                tasks: ['copy:groups']
            },			

            config: {
                files: ['<%= basePath %>/package.manifest'],
                tasks: ['copy:config']
            },
            
            lang: {
                files: ['<%= basePath %>/lang/**'],
                tasks: ['copy:lang']
            }

        },

        copy: {
            config: {
                src: '<%= basePath %>/dist.manifest', // dist.manifest only references the compiled, prod-ready css/js
                dest: '<%= dest %>/<%= basePath %>/package.manifest',
            },

            css: {
                src: '<%= basePath %>/backoffice/css/styles.css',
                dest: '../workflow.site/<%= basePath %>/backoffice/css/styles.min.css', // yes, it's not minified, but the build task will overwrite it later
            },

            js: {
                expand: true,
                cwd: '<%= basePath %>/backoffice/',
                src: '**/*.js',
                dest: '../workflow.site/<%= basePath %>/backoffice/',
            },

            html: {
                expand: true,
                cwd: '<%= basePath %>/backoffice/',
                src: '**/*.html',
                dest: '../workflow.site/<%= basePath %>/backoffice/',
            },

            lang: {
                expand: true,
                cwd: '<%= basePath %>/lang/',
                src: '**',
                dest: '<%= dest %>/<%= basePath %>/lang',
            },

            lib: {
                expand: true,
                cwd: '<%= basePath %>/backoffice/lib/',
                src: '**',
                dest: '<%= dest %>/<%= basePath %>/backoffice/lib/'
            },

            views: {
                expand: true,
                cwd: '<%= basePath %>/backoffice/views/',
                src: '**',
                dest: '<%= dest %>/<%= basePath %>/backoffice/views/'
            },

            approvalGroups: {
                expand: true,
                cwd: '<%= basePath %>/backoffice/approval-groups/',
                src: '**',
                dest: '<%= dest %>/<%= basePath %>/backoffice/approval-groups/'
            },

            partials: {
                expand: true,
                cwd: '<%= basePath %>/backoffice/partials/',
                src: '**',
                dest: '<%= dest %>/<%= basePath %>/backoffice/partials/'
            },
			
            groups: {
                expand: true,
                cwd: '<%= basePath %>/backoffice/approval-groups/',
                src: '**',
                dest: '<%= dest %>/<%= basePath %>/backoffice/approval-groups/'
            },			

            dialogs: {
                expand: true,
                cwd: '<%= basePath %>/backoffice/dialogs/',
                src: '**',
                dest: '<%= dest %>/<%= basePath %>/backoffice/dialogs/'
            },

            nuget: {
                expand: true,
                cwd: '<%= dest %>',
                src: '<%= basePath %>/**',
                dest: 'tmp/nuget/content/'
            },

            umbraco: {
                expand: true,
                cwd: '<%= dest %>/',
                src: '<%= basePath %>/**',
                dest: 'tmp/umbraco/'
            },

            umbracoBin: {
                expand: true,
                cwd: 'bin/Debug/',
                src: 'Workflow.*',
                dest: 'tmp/umbraco/bin'
            },

            testAssets: {
                expand: true,
                cwd: '<%= dest %>',
                src: ['js/umbraco.*.js', 'lib/**/*.js'],
                dest: 'test/assets/'
            }
        },

        template: {
            nuspec: {
                options: {
                    data: {
                        name: '<%= pkg.name %>',
                        version: '<%= pkg.version %>',
                        author: '<%= pkg.author.name %>',
                        description: '<%= pkg.description %>'
                    }
                },
                files: {
                    'tmp/nuget/<%= pkg.name %>.nuspec': 'config/package.nuspec'
                }
            }
        },

        mkdir: {
            pkg: {
                options: {
                    create: ['pkg/nuget', 'pkg/umbraco']
                },
            },
        },

        nugetpack: {
            dist: {
                src: 'tmp/nuget/<%= pkg.name %>.nuspec',
                dest: 'pkg/nuget/'
            }
        },

        umbracoPackage: {
            dist: {
                src: 'tmp/umbraco',
                dest: 'pkg/umbraco',
                options: {
                    name: '<%= pkg.name %>',
                    version: '<%= pkg.version %>',
                    url: '<%= pkg.url %>',
                    license: '<%= pkg.license %>',
                    licenseUrl: '<%= pkg.licenseUrl %>',
                    author: '<%= pkg.author.name %>',
                    authorUrl: '<%= pkg.author.url %>'
                }
            }
        },

        clean: {
            dist: '[object Object]',
            test: 'test/assets'
        },

        karma: {
            unit: {
                configFile: 'test/karma.conf.js'
            }
        },

        jshint: {
            dev: {
                files: {
                    src: ['app_plugins/**/*.js']
                },
                options: {
                    curly: true,
                    eqeqeq: false,
                    immed: true,
                    latedef: false,
                    newcap: false,
                    noarg: true,
                    sub: true,
                    boss: true,
                    eqnull: true,
                    validthis: true,
                    //NOTE: we need to check for strings such as "javascript:" so don't throw errors regarding those
                    scripturl: true,
                    //NOTE: we ignore tabs vs spaces because enforcing that causes lots of errors depending on the text editor being used
                    smarttabs: true,
                    globals: {},
                    force: true,
                    ignores:['**/highcharts.js', '**/exporting.js']
                }
            }
        }
    });

    grunt.registerTask('default', ['jshint', 'concat', 'sass', 'cssmin', 'copy:config', 'copy:groups', 'copy:views', 'copy:approvalGroups', 'copy:partials', 'copy:dialogs', 'copy:lib', 'copy:lang']);
    grunt.registerTask('nuget', ['clean', 'default', 'copy:nuget', 'template:nuspec', 'mkdir:pkg', 'nugetpack']);
    grunt.registerTask('package', ['clean', 'default', 'copy:umbraco', 'copy:umbracoBin', 'mkdir:pkg', 'umbracoPackage']);

    grunt.registerTask('dev', ['watch:dev']);

    grunt.registerTask('test', 'Clean, copy test assets, test', function () {
        var assetsDir = grunt.config.get('dest');
        //copies over umbraco assets from --target, this must point at the /umbraco/ directory
        if (assetsDir !== 'dist') {
            grunt.task.run(['clean:test', 'copy:testAssets', 'karma']);
        } else if (grunt.file.isDir('test/assets/js/')) {
            grunt.log.oklns('Test assets found, running tests');
            grunt.task.run(['karma']);
        } else {
            grunt.log.errorlns('Tests assets not found, skipping tests');
        }
    });

};
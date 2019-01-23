module.exports = grunt => {
    const sass = require('node-sass');

    require('load-grunt-tasks')(grunt);
    require('time-grunt')(grunt);

    //cant load this with require
    grunt.loadNpmTasks('grunt-contrib-jshint');
    grunt.loadNpmTasks('grunt-contrib-cssmin');
    grunt.loadNpmTasks('grunt-banner');

    if (grunt.option('target') && !grunt.file.isDir(grunt.option('target'))) {
        grunt.fail.warn('The --target option specified is not a valid directory');
    }

    grunt.initConfig({
        packageVersion: () => {
            var buildVersion = grunt.option('buildversion') || '1.0.0.1',
                packageSuffix = grunt.option('packagesuffix') || 'build',
                buildBranch = grunt.option('buildbranch') || 'master';

            var findPoint = buildVersion.lastIndexOf('.');
            var basePackageVer = buildVersion.substring(0, findPoint);
            var buildNumber = buildVersion.substring(findPoint + 1, buildVersion.length);
            if (buildBranch.toLowerCase() !== 'release') {
                return basePackageVer + '-build' + buildNumber;
            } else if (packageSuffix !== 'build' && packageSuffix.length > 0) {
                return basePackageVer + '-' + packageSuffix;
            } else {
                return basePackageVer;
            }
        },
        pkg: grunt.file.readJSON('package.json'),
        dest: grunt.option('target') || '../dist',
        basePath: 'App_Plugins/Workflow',
        backoffice: 'App_Plugins/Workflow/backoffice',
        banner:
            '*! <%= pkg.title || pkg.name %> - v<%= packageVersion() %> - <%= grunt.template.today("yyyy-mm-dd") %>\n' +
            '<%= pkg.homepage ? " * " + pkg.homepage + "\\n" : "" %>' +
            ' * Copyright (c) <%= grunt.template.today("yyyy") %> <%= pkg.author.name %>;\n' +
            ' * Licensed <%= pkg.license %>\n *',

        //Concat all the JS files into one
        concat: {
            dist: {
                src: [
                    '<%= backoffice %>/app.js',
                    '<%= backoffice %>/controllers/**/*.js',
                    '<%= backoffice %>/directives/*.js',
                    '<%= backoffice %>/interceptors/*.js',
                    '<%= backoffice %>/resources/*.js',
                    '<%= backoffice %>/filters/*.js'
                ],
                dest: '<%= backoffice %>/workflow.es6',
                nonull: true,
                options: {
                    banner: '/<%= banner %>/\n\n'
                }
            },
            preview: {
                src: [
                    '<%= backoffice %>/preview/app.js',
                    '<%= backoffice %>/preview/workflow.preview.services.js',
                    '<%= backoffice %>/preview/workflow.preview.controller.js',
                    '<%= backoffice %>/controllers/workflow.action.controller.js',
                    '<%= backoffice %>/filters/workflow.iconName.filter.js',
                    '<%= backoffice %>/directives/workflow.comments.directive.js'
                ],
                dest: '<%= backoffice %>/preview.es6',
                nonull: true,
                options: {
                    banner: '/<%= banner %>/\n\n'
                }
            }
        },

        //Compile the less file into a CSS file
        sass: {
            options: {
                implementation: sass,
                sourceMap: false
            },
            dist: {
                files: {
                    '<%= backoffice %>/css/styles.css': ['<%= backoffice %>/css/styles.scss'],
                    '<%= backoffice %>/css/preview.css': ['<%= backoffice %>/preview/styles.scss']
                },
            }
        },

        cssmin: {
            target: {
                files: [{
                    expand: true,
                    cwd: '<%= backoffice %>/css',
                    src: ['*.css', '!*.min.css'],
                    dest: '<%= dest %>/<%= backoffice %>/css',
                    ext: '.min.css'
                }]
            },
            add_banner: {
                files: { 
                    '<%= dest %>/<%= backoffice %>/css/styles.min.css': ['<%= dest %>/<%= backoffice %>/css/styles.min.css'],
                    '<%= dest %>/<%= backoffice %>/css/preview.min.css': ['<%= dest %>/<%= backoffice %>/css/preview.min.css']
                }
            }
        },

        browserify: {
            dist: {
                files: {
                    // destination for transpiled js : source js
                    '<%= dest %>/<%= backoffice %>/js/workflow.js': '<%= backoffice %>/workflow.es6',
                    '<%= dest %>/<%= backoffice %>/js/preview.js': '<%= backoffice %>/preview.es6'
                },
                options: {
                    transform: [['babelify', { presets: ['env'] }]],
                    browserifyOptions: {
                        debug: false
                    }
                }
            }
        },


        watch: {

            // dev watches everything, copies everything
            dev: {
                files: ['<%= backoffice %>/**/*'],
                tasks: ['sass:dist', 'copy:dev', 'copy:configDev'],
                options: {
                    livereload: true
                }
            },

            css: {
                files: ['<%= backoffice %>/css/*.scss', '<%= backoffice %>/preview/*.scss'],
                tasks: ['sass:dist']
            },

            js: {
                files: ['<%= backoffice %>/**/*.js'],
                tasks: ['copy:js']
            },

            html: {
                files: ['<%= backoffice %>/**/*.html'],
                tasks: ['copy:views']
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
            dev: {
                expand: true,
                cwd: '<%= backoffice %>/',
                src: '**/*',
                dest: '../workflow.site/<%= backoffice %>/',
            },

            config: {
                src: '<%= basePath %>/dist.manifest', // dist.manifest only references the compiled, prod-ready css/js
                dest: '<%= dest %>/<%= basePath %>/package.manifest',
            },

            configDev: {
                src: '<%= basePath %>/package.manifest',
                dest: '../workflow.site/<%= basePath %>/package.manifest',
            },

            //css: {
            //    src: '<%= backoffice %>/css/styles.css',
            //    dest: '<%= dest %>/<%= backoffice %>/css/styles.min.css', // yes, it's not minified, but the build task will overwrite it later
            //},

            js: {
                expand: true,
                cwd: '<%= backoffice %>/',
                src: '**/*.js',
                dest: '<%= dest %>/<%= backoffice %>/',
            },

            loader: {
                src: '<%= backoffice %>/preview/preview.loader.prod.js',
                dest: '<%= dest %>/<%= backoffice %>/preview/preview.loader.js'
            },

            html: {
                expand: true,
                cwd: '<%= backoffice %>/',
                src: '**/*.*html',
                dest: '<%= dest %>/<%= backoffice %>/',
            },

            lang: {
                expand: true,
                cwd: '<%= basePath %>/lang/',
                src: '**',
                dest: '<%= dest %>/<%= basePath %>/lang',
            },

            lib: {
                expand: true,
                cwd: '<%= backoffice %>/lib/',
                src: '**',
                dest: '<%= dest %>/<%= backoffice %>/lib/'
            },

            tours: {
                expand: true,
                cwd: '<%= backoffice %>/tours/',
                src: '**',
                dest: '<%= dest %>/<%= backoffice %>/tours/'
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
                    esversion: 6,
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
                    loopfunc: true,
                    ignores: ['**/lib/**/*.js']
                }
            }
        }
    });

    grunt.registerTask('default', ['jshint', 'concat:dist', 'concat:preview', 'browserify:dist', 'sass', 'cssmin', 'copy:loader', 'copy:config', 'copy:tours', 'copy:html', 'copy:lib', 'copy:lang']);
    grunt.registerTask('nuget', ['clean', 'default', 'copy:nuget', 'template:nuspec', 'mkdir:pkg', 'nugetpack']);
    grunt.registerTask('package', ['clean', 'default', 'copy:umbraco', 'copy:umbracoBin', 'mkdir:pkg', 'umbracoPackage']);

    grunt.registerTask('dev', ['watch:dev']);

};

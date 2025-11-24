#!/usr/bin/env perl
use 5.040;
use utf8;
use open ':std', ':encoding(UTF-8)';
use Sq -sig => 1;
use OpenGL::GLFW qw(:all);
use OpenGL::Modern qw(:all);

glfwInit();
glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
#glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GL_TRUE);

my $window = glfwCreateWindow(800, 600, "LearnOpenGL", NULL, NULL);
if ($window == NULL) {
    warn "Failed to create GLFW window\n";
    glfwTerminate();
    exit -1;
}
glfwMakeContextCurrent($window);
glViewport(0, 0, 800, 600);

glfwSetFramebufferSizeCallback($window, sub($window,$width,$height) {
    warn "Resize: $width x $height\n";
    glViewport(0, 0, $width, $height);
});

while(!glfwWindowShouldClose($window)) {
    glClearColor(0.2, 0.3, 0.3, 1.0);
    glClear(GL_COLOR_BUFFER_BIT);

    # Abort Loop on ESC Press
    if( glfwGetKey($window, GLFW_KEY_ESCAPE) == GLFW_PRESS ) {
        glfwSetWindowShouldClose($window, true);
    }

    glfwPollEvents();
    glfwSwapBuffers($window);
}

glfwTerminate();
exit 0;

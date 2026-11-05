#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;

uniform mat4 model; // local coordinates
uniform mat4 projection;
uniform mat4 view; // world coordinates for cam

uniform vec3 ObjColor; // vertex color

// send to fragment shader for shading
out vec3 FragPos;   
out vec3 Normal;
out vec3 Color;

void main()
{

    vec4 worldPos = model * vec4(aPos, 1.0);
    FragPos = vec3(model * vec4(aPos, 1.0)); // pass world position to fragment shader
    Color = ObjColor;

    gl_Position = projection * view * worldPos;
    Normal = mat3(transpose(inverse(model))) * aNormal; // transform normal to world space
    //Normal = aNormal; // if model matrix is only translation, normal doesn't need to be transformed
}
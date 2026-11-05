#version 330 core
out vec4 FragColor;

in vec3 FragPos;
in vec3 Normal;
in vec3 Color;


uniform vec3 lightPos;

void main()
{
 
    // shape colour
    FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
    // simple shading using dot product
    //FragColor = vec4(vec3(dot(normalize(FragPos), vec3(1,1,1))), 1.0) * vec4(1.0f, 0.5f, 0.2f, 1.0f);

    float ambientStrength = 0.1;
    vec3 lightColor = vec3(1.0f, 0.5f, 0.2f);
    //vec3 objectColor = vec3(1.0f, 0.5f, 0.2f);
    vec3 objectColor = Color;

    // find Normal and light direction
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(lightPos - FragPos);  
    // diffuse calc
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor;

    vec3 ambient = ambientStrength * lightColor;

    vec3 result = (ambient + diffuse) * objectColor;
    FragColor = vec4(result, 1.0);
}
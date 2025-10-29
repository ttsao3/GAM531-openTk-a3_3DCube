#version 330 core
out vec4 FragColor;
 
in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;
 
uniform vec3 lightPos[3]; // Position of the point light
uniform vec3 viewPos;  // Camera position
uniform vec3 lightColor; // Color of the light
uniform sampler2D texture0; //bound texture
uniform bool lightOn[3]; //if light is on
 
void main()
{
    vec3 norm = normalize(Normal);
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 result = vec3(0.0);

    for (int i = 0; i < 3; i++){
        if (!lightOn[i]) continue;

        vec3 lightDir = normalize(lightPos[i] - FragPos);

        // Ambient
        float ambientStrength = 0.1;
        vec3 ambient = ambientStrength * lightColor;
 
        // Diffuse
        float diff = max(dot(norm, lightDir), 0.0);
        vec3 diffuse = diff * lightColor;
 
        // Specular
        float specularStrength = 0.5;
        vec3 reflectDir = reflect(-lightDir, norm);
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
        vec3 specular = specularStrength * spec * lightColor;

        result += (ambient + diffuse + specular);
    }
    
    vec3 texColor = texture(texture0, TexCoord).rgb;
    FragColor = vec4(result * texColor, 1.0);
}

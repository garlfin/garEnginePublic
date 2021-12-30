#version 330

uniform sampler2D albedo;
uniform samplerCube cubemap;

in vec3 FragPos;
in vec2 fTexCoord;
in vec3 fNormal;

uniform vec3 viewVec;
uniform vec3 lightPos;

out vec4 colorOut;

vec4 mixMultiply(vec4 inone, vec4 intwo, float mixfac){
    return mix(inone, inone*intwo, mixfac);
}

void main() {
    vec4 color = vec4(1); 
    float specFactor = 1 - 0.75;
    vec3 lightDir = normalize(lightPos);  
    float ambient = max(dot(lightDir,fNormal),0)*0.5+0.5;
    vec3 viewPos = normalize(viewVec-FragPos);
    float fresnel = clamp(1 - max(dot(viewPos, fNormal),0),0,1);
    // light reflected off normal, dot product with view vector
    float spec = clamp(pow(max(0,dot(reflect(lightDir, fNormal),-viewPos)),pow(12,1+specFactor)),0,1)*specFactor; 
    color = pow(texture(albedo, fTexCoord)*ambient+vec4(spec), vec4(1.0/2.2));
    color = mixMultiply(color, texture(cubemap, reflect(-viewPos, fNormal)), fresnel * specFactor);
    colorOut = color;
    
    //colorOut = texture(cubemap, reflect(-viewPos, fNormal));
}
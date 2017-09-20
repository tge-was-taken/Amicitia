#version 330

out vec4 outColor;

in Data
{
  vec3 pos;
  vec3 nrm;
  vec2 tex;
  vec4 col;
} id;

uniform sampler2D diffuse;
uniform vec4 diffuseColor;
uniform int isTextured;

void main()
{
  vec4 color = vec4(1, 1, 1, 1);
  if(isTextured != 0)
  {
    //if(texture2D(diffuse, id.tex).a < .3) discard;
    color = texture2D(diffuse, id.tex);
  }

  //outColor = color;
  outColor = vec4((color * clamp((dot(-vec3(0, 0, 1), id.nrm) + vec4(0.4, 0.4, 0.4, 1)), 0.0, 1.0)).rgb, color.a);
  //outColor = vec4((color*diffuseColor*clamp(dot(-vec3(0, 0, 1), id.nrm), 0.0, 1.0)).rgb, color.a);
}

#version 450

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

void main()
{
  if(texture2D(diffuse, id.tex).a < .1) discard;
  outColor = (texture2D(diffuse, id.tex)*diffuseColor*clamp(dot(-vec3(0, 0, 1), id.nrm), 0.0, 1.0));
}

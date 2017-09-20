#version 330
layout(location = 0) in vec3 pos;
layout(location = 1) in vec3 nrm;
layout(location = 2) in vec2 tex;
layout(location = 3) in vec4 col;

out Data
{
  vec3 pos;
  vec3 nrm;
  vec2 tex;
  vec4 col;
} od;

uniform mat4 proj;
uniform mat4 view;
uniform mat4 tran;

void main()
{
  od.pos = pos;
  od.nrm = (proj * view * tran * vec4(nrm, 0.0) ).xyz;
  od.tex = tex;
  od.col = col;
  gl_Position = proj * view * tran * vec4(pos, 1.0);
}

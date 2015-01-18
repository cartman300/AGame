#version 150

uniform mat4 Matrix;

in vec2 vert_pos;
in vec2 vert_uv;
in vec4 vert_clr;

out vec2 frag_uv;
out vec4 frag_clr;

void main() {
	frag_uv = vert_uv;
	frag_clr = vert_clr;

	gl_Position = Matrix * vec4(vert_pos, 0.0, 1.0);
}
layout(triangles) in;
layout(triangle_strip, max_vertices = 9) out;

//#define MAP(a, b) a = b[i]

void map(int i) {
	///MAPPINGS
}

void main() {
	for(int i = 0; i < 3; i++) {
		map(i);
		gl_Position = gl_in[i].gl_Position;
		EmitVertex();
	}
	EndPrimitive();
}
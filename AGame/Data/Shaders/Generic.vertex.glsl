vec4 tovec4(typeof__pos);

void main() {
	///MAPPINGS
	gl_Position = Matrix * tovec4(__pos);
}
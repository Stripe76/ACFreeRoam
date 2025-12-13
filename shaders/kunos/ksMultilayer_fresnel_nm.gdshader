shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley;

uniform vec4 albedo : source_color = vec4(1,1,1,1);

uniform sampler2D txDiffuse : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D txMask : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D txDetailR : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D txDetailG : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D txDetailB : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D txDetailA : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D txDetailNM : source_color, filter_linear_mipmap, repeat_enable;

uniform float ksAmbient = 1.0;
uniform float ksDiffuse = 1.0;
uniform float ksSpecular = 1.0;
uniform float ksSpecularEXP = 0.0;
uniform float ksEmissive = 0.0;
uniform float ksAlphaRef = 0.0;
uniform float multR = 1.0;
uniform float multG = 1.0;
uniform float multB = 1.0;
uniform float multA = 1.0;
uniform float magicMult = 1.0;


varying vec3 uv1_triplanar_pos;

uniform vec3 uv1_scale;
uniform vec3 uv1_offset;

void vertex() {
	vec3 normal = NORMAL;

	TANGENT = vec3(0.0, 0.0, -1.0) * abs(normal.x);
	TANGENT += vec3(1.0, 0.0, 0.0) * abs(normal.y);
	TANGENT += vec3(1.0, 0.0, 0.0) * abs(normal.z);
	TANGENT = normalize(TANGENT);

	BINORMAL = vec3(0.0, 1.0, 0.0) * abs(normal.x);
	BINORMAL += vec3(0.0, 0.0, -1.0) * abs(normal.y);
	BINORMAL += vec3(0.0, 1.0, 0.0) * abs(normal.z);
	BINORMAL = normalize(BINORMAL);

	// UV1 Triplanar: Enabled
	uv1_triplanar_pos = VERTEX * uv1_scale + uv1_offset;
	//uv1_triplanar_pos *= vec3(1.0, -1.0, 1.0);
}

vec4 triplanar_texture(sampler2D p_sampler, vec3 p_triplanar_pos) {
	return texture(p_sampler,p_triplanar_pos.xz);
}

void fragment() {
	vec2 base_uv = UV;
	
	vec4 mask = texture(txMask,base_uv);
	vec4 diffuse = texture(txDiffuse,base_uv);
	
	vec4 upos = INV_PROJECTION_MATRIX * vec4(SCREEN_UV * 2.0 - 1.0, FRAGCOORD.z, 1.0);
	vec3 pixel_position = upos.xyz / upos.w;
	float dist  = -pixel_position.z / 20.0;
	dist *= dist;
	if( dist > 1.0 )
		dist = 1.0;
		
	vec3 color = diffuse.rgb;
	//color = mix(triplanar_texture(txDetailR,uv1_triplanar_pos).rgb,color,0.5+(dist/2.0));
	color += triplanar_texture(txDetailR,uv1_triplanar_pos).rgb * .2 * (1.0-dist);
	//color = mix(color,texture(txDetailR,detail_uv).rgb,mask.r);
	//color = mix(color,texture(txDetailG,detail_uv).rgb,mask.g);
	//color = mix(color,texture(txDetailB,detail_uv).rgb,mask.b);
	ALBEDO = color;
	//ALBEDO = albedo_tex.rgb;

	//float metallic_tex = dot(texture(texture_metallic, base_uv), metallic_texture_channel);
	//METALLIC = metallic_tex * metallic;
	//SPECULAR = specular;
	
	//vec4 roughness_texture_channel = vec4(1.0, 0.0, 0.0, 0.0);
	//float roughness_tex = dot(texture(texture_roughness, base_uv), roughness_texture_channel);
	//ROUGHNESS = roughness_tex * roughness;
	
	//NORMAL_MAP = texture(texture_normal, base_uv).rgb;
	//NORMAL_MAP_DEPTH = normal_scale;
	
	//ALPHA = albedo_tex.a;
	//ALPHA_SCISSOR_THRESHOLD = 0.1;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}

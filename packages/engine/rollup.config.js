import nodeResolve from "@rollup/plugin-node-resolve";

export default {
  input: './build/index.js',
  output: {
    dir: 'dist',
    format: 'umd',
    name: 'game'
  },
  plugins: [nodeResolve()],
}
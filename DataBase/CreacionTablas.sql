CREATE TABLE provincia
(
  id_provincia INT IDENTITY (1, 1) NOT NULL,
  nombre VARCHAR(50) NOT NULL,
  CONSTRAINT pk_provincia PRIMARY KEY (id_provincia)
);

CREATE TABLE ciudad
(
  id_ciudad INT IDENTITY (1, 1) NOT NULL,
  nombre VARCHAR(50) NOT NULL,
  id_provincia INT NOT NULL,
  CONSTRAINT pk_ciudad PRIMARY KEY (id_ciudad),
  CONSTRAINT fk_provincia FOREIGN KEY (id_provincia) REFERENCES provincia(id_provincia)
);

CREATE TABLE direccion
(
  id_direccion INT IDENTITY (1, 1) NOT NULL,
  nombreCalle VARCHAR(100) NOT NULL,
  altura INT NOT NULL,
  codigo_postal INT NOT NULL,
  id_ciudad INT NOT NULL,
  CONSTRAINT pk_direccion PRIMARY KEY (id_direccion),
  CONSTRAINT fk_ciudad FOREIGN KEY (id_ciudad) REFERENCES ciudad(id_ciudad)
);

CREATE TABLE cliente
(
  dni_cliente INT NOT NULL,
  nombre VARCHAR(100) NOT NULL,
  apellido VARCHAR(100) NOT NULL,
  telefono VARCHAR(20) NOT NULL,
  email VARCHAR(50) NOT NULL,
  CONSTRAINT pk_cliente PRIMARY KEY (dni_cliente)
);

CREATE TABLE direccion_cliente
(
  id_direccion INT NOT NULL,
  dni_cliente INT NOT NULL,
  CONSTRAINT pk_direccion_cliente PRIMARY KEY (id_direccion, dni_cliente),
  CONSTRAINT fk_direccion FOREIGN KEY (id_direccion) REFERENCES direccion(id_direccion),
  CONSTRAINT fk_cliente FOREIGN KEY (dni_cliente) REFERENCES cliente(dni_cliente)
);

CREATE TABLE usuario
(
  id_usuario INT IDENTITY (1, 1) NOT NULL,
  nombre VARCHAR(50) NOT NULL,
  apellido VARCHAR(50) NOT NULL,
  dni INT NOT NULL,
  email VARCHAR(50) NOT NULL,
  contraseña VARCHAR(50) NOT NULL,
  baja CHAR(2) NOT NULL,
  CONSTRAINT pk_usuario PRIMARY KEY (id_usuario),
  CONSTRAINT uq_dni UNIQUE (dni),
  CONSTRAINT uq_email UNIQUE (email)
);

INSERT INTO usuario (id_usuario, nombre, apellido, dni, email, contraseña, baja) VALUES (1, 'Bianca', 'Isasi', 43068676, 'bianca@gmail', '123456', 'no')

--SELECT * FROM usuario;

CREATE TABLE estado
(
  id_estado INT IDENTITY (1, 1) NOT NULL,
  nombre VARCHAR(15) NOT NULL,
  CONSTRAINT pk_estado PRIMARY KEY (id_estado)
);

CREATE TABLE transporte
(
  id_transporte INT IDENTITY (1, 1) NOT NULL,
  nombre VARCHAR(50) NOT NULL,
  CONSTRAINT pk_transporte PRIMARY KEY (id_transporte)
);

CREATE TABLE categoria
(
  id_categoria INT IDENTITY (1, 1) NOT NULL,
  nombre VARCHAR(20) NOT NULL,
  descripcion VARCHAR(150) NOT NULL,
  CONSTRAINT pk_categoria PRIMARY KEY (id_categoria)
);

CREATE TABLE tipo_usuario
(
  id_tipo_usuario INT IDENTITY (1, 1) NOT NULL,
  tipo VARCHAR(10) NOT NULL,
  descripcion VARCHAR(100) NOT NULL,
  CONSTRAINT pk_tipo_usuario PRIMARY KEY (id_tipo_usuario)
);

CREATE TABLE venta
(
  id_nro_venta INT IDENTITY (1, 1) NOT NULL,
  fechaHora DATE NOT NULL,
  total FLOAT NOT NULL,
  dni_cliente INT NOT NULL,
  id_usuario INT,
  CONSTRAINT pk_venta PRIMARY KEY (id_nro_venta),
  CONSTRAINT fk_cliente FOREIGN KEY (dni_cliente) REFERENCES cliente(dni_cliente),
  CONSTRAINT fk_usuario FOREIGN KEY (id_usuario) REFERENCES usuario(id_usuario)
);

CREATE TABLE envio
(
  id_envio INT IDENTITY (1, 1) NOT NULL,
  fecha_despacho DATE NOT NULL,
  num_seguimiento VARCHAR(25) NOT NULL,
  costo FLOAT NOT NULL,
  id_estado INT NOT NULL,
  id_transporte INT NOT NULL,
  id_nro_venta INT NOT NULL,
  CONSTRAINT pk_envio PRIMARY KEY (id_envio),
  CONSTRAINT fk_estado FOREIGN KEY (id_estado) REFERENCES estado(id_estado),
  CONSTRAINT fk_transporte FOREIGN KEY (id_transporte) REFERENCES transporte(id_transporte),
  CONSTRAINT fk_nro_venta FOREIGN KEY (id_nro_venta) REFERENCES venta(id_nro_venta)
);

CREATE TABLE producto
(
  id_producto INT IDENTITY (1, 1) NOT NULL,
  nombre VARCHAR(50) NOT NULL,
  descripcion VARCHAR(150) NOT NULL,
  precio FLOAT NOT NULL,
  cantidad INT NOT NULL,
  stock_minimo INT NOT NULL,
  baja CHAR(2) NOT NULL,
  id_categoria INT NOT NULL,
  CONSTRAINT pk_producto PRIMARY KEY (id_producto),
  CONSTRAINT fk_categoria FOREIGN KEY (id_categoria) REFERENCES categoria(id_categoria)
);

CREATE TABLE funcion_usuario
(
  id_tipo_usuario INT NOT NULL,
  id_usuario INT NOT NULL,
  CONSTRAINT pk_funcion_usuario PRIMARY KEY (id_tipo_usuario, id_usuario),
  CONSTRAINT fk_tipo_usuario FOREIGN KEY (id_tipo_usuario) REFERENCES tipo_usuario(id_tipo_usuario),
  CONSTRAINT fk_id_usuario FOREIGN KEY (id_usuario) REFERENCES usuario(id_usuario)
);

CREATE TABLE detalle_venta_producto
(
  id_detalle INT IDENTITY (1, 1) NOT NULL,
  cantidad INT NOT NULL,
  subtotal FLOAT NOT NULL,
  id_nro_venta INT NOT NULL,
  id_producto INT NOT NULL,
  CONSTRAINT pk_detalle_venta_producto PRIMARY KEY (id_detalle),
  CONSTRAINT fk_nro_venta FOREIGN KEY (id_nro_venta) REFERENCES venta(id_nro_venta),
  CONSTRAINT fk_producto FOREIGN KEY (id_producto) REFERENCES producto(id_producto)
);
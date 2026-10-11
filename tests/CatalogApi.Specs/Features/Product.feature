# language: es
Característica: Gestión de productos
  Como administrador del catálogo
  Quiero administrar los productos
  Para mantener el catálogo organizado y evitar duplicados

  Antecedentes:
    Dado que el catálogo no tiene productos
    Y existe una categoría llamada "Bebidas"

  Escenario: Crear un producto con datos válidos
    Cuando creo un producto con los siguientes datos:
      | Nombre      | Coca-Cola        |
      | Descripción | Refresco de cola |
      | Categoría   | Bebidas          |
    Entonces el producto "Coca-Cola" queda registrado
    Y puedo consultarlo en el listado de productos

  Escenario: Crear un producto sin descripción
    Cuando creo un producto con los siguientes datos:
      | Nombre    | Coca-Cola |
      | Categoría | Bebidas   |
    Entonces el producto "Coca-Cola" queda registrado

  Escenario: No se puede crear un producto sin nombre
    Cuando intento crear un producto sin nombre
    Entonces la creación es rechazada
    Y se informa que el nombre es obligatorio

  Escenario: No se puede crear un producto sin categoría
    Cuando intento crear un producto sin categoría
    Entonces la creación es rechazada
    Y se informa que la categoría es obligatoria

  Escenario: No se puede crear un producto con una categoría que no existe
    Cuando creo un producto con los siguientes datos:
      | Nombre    | Coca-Cola |
      | Categoría | Fantasma  |
    Entonces la creación es rechazada
    Y se informa que la categoría no existe

  Escenario: No se puede repetir el nombre ignorando mayúsculas y minúsculas
    Dado que existe un producto llamado "Coca-Cola"
    Cuando intento crear un producto llamado "COCA-COLA"
    Entonces la creación es rechazada
    Y se informa que el nombre ya está en uso

  Escenario: Consultar el listado de productos
    Dado que existe un producto llamado "Coca-Cola"
    Cuando consulto el listado de productos
    Entonces veo el producto "Coca-Cola"

  Escenario: Consultar un producto muestra el nombre de su categoría
    Dado que existe un producto llamado "Coca-Cola" en la categoría "Bebidas"
    Cuando consulto el producto "Coca-Cola"
    Entonces veo el producto "Coca-Cola"
    Y veo el nombre de la categoría "Bebidas"

  Escenario: Se puede editar un producto existente
    Dado que existe un producto llamado "Coca-Cola"
    Cuando edito el producto "Coca-Cola" con los siguientes datos:
      | Nombre      | Coca-Cola Zero      |
      | Descripción | Refresco sin azúcar |
      | Categoría   | Bebidas             |
    Entonces el producto queda actualizado con el nuevo nombre, descripción y categoría
    Y puedo consultarlo en el listado de productos

  Escenario: Se puede cambiar la categoría de un producto
    Dado que existe un producto llamado "Coca-Cola" en la categoría "Bebidas"
    Y existe una categoría llamada "Snacks"
    Cuando edito el producto "Coca-Cola" con los siguientes datos:
      | Nombre      | Coca-Cola        |
      | Descripción | Refresco de cola |
      | Categoría   | Snacks           |
    Entonces el producto queda en la categoría "Snacks"

  Escenario: No se puede editar un producto dejándolo sin nombre
    Dado que existe un producto llamado "Coca-Cola"
    Cuando intento editar el producto "Coca-Cola" sin nombre
    Entonces la edición es rechazada
    Y se informa que el nombre es obligatorio

  Escenario: No se puede renombrar un producto a un nombre ya usado
    Dado que existe un producto llamado "Coca-Cola"
    Y existe un producto llamado "Sprite"
    Cuando intento renombrar el producto "Sprite" a "COCA-COLA"
    Entonces la edición es rechazada
    Y se informa que el nombre ya está en uso

  Escenario: No se puede editar un producto que no existe
    Cuando intento editar el producto "Fantasma" con los siguientes datos:
      | Nombre    | Fantasma |
      | Categoría | Bebidas  |
    Entonces la edición es rechazada
    Y se informa que el producto no existe

  Escenario: Eliminar un producto existente
    Dado que existe un producto llamado "Coca-Cola"
    Cuando elimino el producto "Coca-Cola"
    Entonces el producto queda eliminado del catálogo
    Y no puedo consultarlo en el listado de productos

  Escenario: No se puede eliminar un producto que no existe
    Cuando intento eliminar el producto "Fantasma"
    Entonces la eliminación es rechazada
    Y se informa que el producto no existe

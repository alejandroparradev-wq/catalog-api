# language: es
Característica: Gestión de categorías
  Como administrador del catálogo
  Quiero administrar las categorías de productos
  Para mantener el catálogo organizado y evitar duplicados

  Antecedentes:
    Dado que el catálogo no tiene categorías

  Escenario: Crear una categoría con datos válidos
    Cuando creo una categoría con los siguientes datos:
      | Nombre      | Bebidas           |
      | Descripción | Refrescos y jugos |
    Entonces la categoría "Bebidas" queda registrada
    Y puedo consultarla en el listado de categorías

  Escenario: Crear una categoría sin descripción
    Cuando creo una categoría con los siguientes datos:
      | Nombre | Bebidas |
    Entonces la categoría "Bebidas" queda registrada

  Escenario: No se puede crear una categoría sin nombre
    Cuando intento crear una categoría sin nombre
    Entonces la creación es rechazada
    Y se informa que el nombre es obligatorio

  Escenario: No se puede repetir el nombre ignorando mayúsculas y minúsculas
    Dado que existe una categoría llamada "Bebidas"
    Cuando intento crear una categoría llamada "BEBIDAS"
    Entonces la creación es rechazada
    Y se informa que el nombre ya está en uso

  Escenario: Consultar el listado de categorías
    Dado que existe una categoría llamada "Bebidas"
    Y existe una categoría llamada "Alimentos"
    Cuando consulto el listado de categorías
    Entonces veo las categorías "Bebidas" y "Alimentos"

  Escenario: Consultar el detalle de una categoría
    Dado que existe una categoría llamada "Bebidas"
    Cuando consulto la categoría "Bebidas"
    Entonces veo el detalle de la categoría "Bebidas"

  Escenario: Se puede editar una categoría existente
    Dado que existe una categoría llamada "Bebidas"
    Cuando edito la categoría "Bebidas" con los siguientes datos:
      | Nombre      | Bebidas y Jugos   |
      | Descripción | Refrescos y jugos |
    Entonces la categoría queda actualizada con el nuevo nombre y descripción
    Y puedo consultarla en el listado de categorías

  Escenario: No se puede renombrar una categoría a un nombre ya usado
    Dado que existe una categoría llamada "Bebidas"
    Y existe una categoría llamada "Alimentos"
    Cuando intento renombrar la categoría "Alimentos" a "BEBIDAS"
    Entonces la edición es rechazada
    Y se informa que el nombre ya está en uso

  Escenario: No se puede editar una categoría dejándola sin nombre
    Dado que existe una categoría llamada "Bebidas"
    Cuando intento editar la categoría "Bebidas" sin nombre
    Entonces la edición es rechazada
    Y se informa que el nombre es obligatorio

  Escenario: No se puede editar una categoría que no existe
    Cuando intento editar la categoría "Fantasma" con los siguientes datos:
      | Nombre | Fantasma |
    Entonces la edición es rechazada
    Y se informa que la categoría no existe

  Escenario: Eliminar una categoría sin productos
    Dado que existe una categoría llamada "Bebidas"
    Cuando elimino la categoría "Bebidas"
    Entonces la categoría queda eliminada
    Y no puedo consultarla en el listado de categorías

  Escenario: No se puede eliminar una categoría que tiene productos asignados
    Dado que existe una categoría llamada "Bebidas" con productos asignados
    Cuando intento eliminar la categoría "Bebidas"
    Entonces la eliminación es rechazada
    Y se informa que la categoría tiene productos asignados

  Escenario: Se puede eliminar una categoría cuando ya no tiene productos
    Dado que existía una categoría llamada "Bebidas" cuyo último producto fue eliminado
    Cuando elimino la categoría "Bebidas"
    Entonces la categoría queda eliminada

  Escenario: No se puede eliminar una categoría que no existe
    Cuando intento eliminar la categoría "Fantasma"
    Entonces la eliminación es rechazada
    Y se informa que la categoría no existe

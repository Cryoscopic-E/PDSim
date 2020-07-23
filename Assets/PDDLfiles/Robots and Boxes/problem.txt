(define (problem robot_movement) 

    (:domain robot)

    (:objects 
        a b c d e f - room
        robot1 robot2 - robot
        box1 box2 box3 - box
    )

    (:init
       (at_location robot2 a)
       (at_location robot1 d)
       (at_location box1 f)
       (at_location box2 b)
       (at_location box3 e)
              (path a c)
              (path c a)
              
              (path a f)
              (path f a)
              
              (path f e)
              (path e f)
              
              (path c d)
              (path d c)
              
              (path b d)
              (path d b)
              
              (path b e)
              (path e b)
    )

    (:goal 
        (and
            (at_location box1 a)
            (at_location box2 c)
            (at_location box3 f)
        )
    )
)